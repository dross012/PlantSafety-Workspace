// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Infrastructure
//  File:           ComplianceEvaluationDapperRepository.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IComplianceEvaluationRepository. All DB
//                  access via stored procedures in the [ComplianceRulesEngine] schema. Aggregate
//                  reconstruction uses AggregateReflectionHydrator (mirrors
//                  RiskDapperRepository's precedent) because ComplianceEvaluation exposes only a
//                  private rehydration constructor and private-setter properties. ListAsync
//                  evaluates the supplied ComplianceEvaluationSpecification in-memory over the
//                  full tenant candidate set, matching the same precedent.
//
//                  AddAsync also relays ComplianceViolationDetectedEvent/
//                  ComplianceEvaluationCompletedEvent onto Shared.EventBus via
//                  ICrossModuleEventRelay (Publish-After-Commit, same discipline as
//                  UnitOfWork.CommitAsync), in addition to the existing IOutboxWriter enqueue --
//                  this repository does not go through TransactionBehavior/UnitOfWork (it owns
//                  its own connection/transaction), so it must drive the relay itself. Additive-
//                  only evolution.
// ============================================================================
using System.Data;
using System.Text.Json;
using Dapper;
using PlantSafety.Domain.Compliance.RulesEngine.Aggregates;
using PlantSafety.Domain.Compliance.RulesEngine.Entities;
using PlantSafety.Domain.Compliance.RulesEngine.Enums;
using PlantSafety.Domain.Compliance.RulesEngine.Events;
using PlantSafety.Domain.Compliance.RulesEngine.Interfaces;
using PlantSafety.Domain.Compliance.RulesEngine.Specifications;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects;
using PlantSafety.Domain.Compliance.RulesEngine.ValueObjects.Ids;
using PlantSafety.Infrastructure.Messaging.CrossModule;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Compliance.RulesEngine.Persistence;

/// <summary>Dapper-backed repository for the <see cref="ComplianceEvaluation"/> aggregate root.</summary>
public sealed class ComplianceEvaluationDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ComplianceEvaluationDapperRepository> logger,
    ICrossModuleEventRelay? crossModuleEventRelay = null)
    : IComplianceEvaluationRepository
{
    private const string RuleResultTableType = "[ComplianceRulesEngine].[ComplianceRuleResultTableType]";

    /// <inheritdoc/>
    public async Task<ComplianceEvaluation?> GetByIdAsync(ComplianceEvaluationId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("ComplianceEvaluationId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<EvaluationRow>(
            "[ComplianceRulesEngine].[usp_ComplianceEvaluation_GetById]", parameters, cancellationToken);

        if (row is null) return null;

        var ruleResultRows = await GetRuleResultRowsAsync(id, cancellationToken);
        return Hydrate(row, ruleResultRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ComplianceEvaluation>> ListAsync(Guid tenantId, ComplianceEvaluationSpecification specification, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<EvaluationRow>(
            "[ComplianceRulesEngine].[usp_ComplianceEvaluation_GetAllForTenant]", parameters, cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<ComplianceEvaluation>();

        foreach (var row in rows)
        {
            var ruleResultRows = await GetRuleResultRowsAsync(ComplianceEvaluationId.FromGuid(row.ComplianceEvaluationId), cancellationToken);
            var aggregate = Hydrate(row, ruleResultRows);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ComplianceEvaluation>> GetHistoryForTargetAsync(Guid tenantId, string targetEntityType, Guid targetEntityId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("TargetEntityType", targetEntityType, 200)
            .AddGuid("TargetEntityId", targetEntityId)
            .Build();

        var rows = await queryExecutor.QueryAsync<EvaluationRow>(
            "[ComplianceRulesEngine].[usp_ComplianceEvaluation_GetHistoryForTarget]", parameters, cancellationToken);

        var results = new List<ComplianceEvaluation>();
        foreach (var row in rows)
        {
            var ruleResultRows = await GetRuleResultRowsAsync(ComplianceEvaluationId.FromGuid(row.ComplianceEvaluationId), cancellationToken);
            results.Add(Hydrate(row, ruleResultRows));
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(ComplianceEvaluation aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add ComplianceEvaluation {ComplianceEvaluationId}", aggregate.ComplianceEvaluationId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var evalParams = new StoredProcedureBuilder()
            .AddGuid("ComplianceEvaluationId", aggregate.ComplianceEvaluationId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("TargetEntityType", aggregate.TargetEntityType, 200)
            .AddGuid("TargetEntityId", aggregate.TargetEntityId)
            .AddString("Status", aggregate.Status.Code, 30)
            .AddDateTime("StartedAtUtc", aggregate.StartedAtUtc.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddGuid("TriggeredBy", aggregate.TriggeredBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[ComplianceRulesEngine].[usp_ComplianceEvaluation_Insert]", evalParams, tx, cancellationToken: cancellationToken);

        // Bulk-insert every rule result in a single set-based round trip (Handbook §7 Performance
        // & Stress -- Phase 5 §7.3 "bulk operations") instead of one usp_ComplianceRuleResult_Insert
        // call per rule -- a single evaluation run can evaluate dozens to hundreds of rules against
        // one target entity. See usp_ComplianceRuleResult_BulkInsert.sql.
        if (aggregate.RuleResults.Count > 0)
        {
            var ruleResultParameters = new DynamicParameters();
            ruleResultParameters.Add("ComplianceEvaluationId", aggregate.ComplianceEvaluationId.Value);
            ruleResultParameters.Add("RuleResults", BuildRuleResultTable(aggregate.RuleResults).AsTableValuedParameter(RuleResultTableType));

            await commandExecutor.ExecuteAsync(conn, "[ComplianceRulesEngine].[usp_ComplianceRuleResult_BulkInsert]", ruleResultParameters, tx, cancellationToken: cancellationToken);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
            crossModuleEventRelay?.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();

        // Publish-After-Commit: the SQL transaction above has already committed, so it is safe to
        // relay onto Shared.EventBus now (mirrors UnitOfWork.CommitAsync's discipline).
        if (crossModuleEventRelay is { HasPendingEvents: true })
            await crossModuleEventRelay.FlushAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public Task UpdateAsync(ComplianceEvaluation aggregate, CancellationToken cancellationToken = default)
        // ComplianceEvaluation is currently write-once (Start -> RecordRuleResult* -> Complete all
        // happen in-memory before AddAsync is ever called by ComplianceEvaluator). UpdateAsync is
        // provided to satisfy the domain repository contract for any future scenario that persists
        // a partially-evaluated run and completes it later; re-inserting via AddAsync's shape would
        // violate the primary key, so this is intentionally a no-op until such a scenario exists.
        => Task.CompletedTask;

    /// <summary>
    /// Builds the TVP rows for <c>usp_ComplianceRuleResult_BulkInsert</c>. Uses each result's own
    /// <see cref="ComplianceRuleResult.ComplianceRuleResultId"/> (assigned by the entity's
    /// Pass/Fail factory at construction) rather than minting a fresh id at persistence time --
    /// correcting a latent inconsistency in the previous per-row insert loop, which generated a
    /// new <c>Uuid7</c> per call instead of using the entity's already-assigned identity.
    /// </summary>
    private static DataTable BuildRuleResultTable(IReadOnlyCollection<ComplianceRuleResult> ruleResults)
    {
        var table = new DataTable();
        table.Columns.Add("ComplianceRuleResultId", typeof(Guid));
        table.Columns.Add("RuleId", typeof(Guid));
        table.Columns.Add("RuleCode", typeof(string));
        table.Columns.Add("RuleName", typeof(string));
        table.Columns.Add("Category", typeof(string));
        table.Columns.Add("Passed", typeof(bool));
        table.Columns.Add("ViolationsJson", typeof(string));
        table.Columns.Add("EvaluatedAtUtc", typeof(DateTime));

        foreach (var result in ruleResults)
        {
            var violationsJson = JsonSerializer.Serialize(result.Violations.Select(v => new
            {
                v.ViolationCode,
                v.Description,
                Severity = v.Severity.Code,
                v.RemediationGuidance,
            }));

            table.Rows.Add(
                result.ComplianceRuleResultId.Value,
                result.RuleId.Value,
                result.RuleCode,
                result.RuleName,
                result.Category.Code,
                result.Passed,
                violationsJson,
                result.EvaluatedAtUtc.Value);
        }

        return table;
    }

    private async Task<IReadOnlyList<RuleResultRow>> GetRuleResultRowsAsync(ComplianceEvaluationId id, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("ComplianceEvaluationId", id.Value).Build();
        return await queryExecutor.QueryAsync<RuleResultRow>(
            "[ComplianceRulesEngine].[usp_ComplianceRuleResult_GetByEvaluationId]", parameters, cancellationToken);
    }

    private static ComplianceEvaluation Hydrate(EvaluationRow row, IReadOnlyList<RuleResultRow> ruleResultRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<ComplianceEvaluation>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceEvaluation.ComplianceEvaluationId), ComplianceEvaluationId.FromGuid(row.ComplianceEvaluationId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceEvaluation.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceEvaluation.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceEvaluation.TargetEntityType), row.TargetEntityType);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceEvaluation.TargetEntityId), row.TargetEntityId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceEvaluation.Status), ComplianceEvaluationStatus.From(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceEvaluation.StartedAtUtc), UtcTimestamp.From(row.StartedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceEvaluation.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ComplianceEvaluation.TriggeredBy), new UserId(row.TriggeredBy));

        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IComplianceRuleEngineDomainEvent>());
        AggregateReflectionHydrator.SetField(agg, "_ruleResults", new List<ComplianceRuleResult>());

        var resultList = AggregateReflectionHydrator.GetPrivateCollectionField<ComplianceEvaluation, List<ComplianceRuleResult>>(agg, "_ruleResults");
        foreach (var r in ruleResultRows)
        {
            var violations = (JsonSerializer.Deserialize<List<ViolationJson>>(r.ViolationsJson) ?? [])
                .Select(v => ComplianceViolation.Create(v.ViolationCode, v.Description, ComplianceSeverity.From(v.Severity), v.RemediationGuidance))
                .ToList();

            var child = r.Passed
                ? ComplianceRuleResult.Pass(ComplianceEvaluationId.FromGuid(r.ComplianceEvaluationId), ComplianceRuleId.FromGuid(r.RuleId), r.RuleCode, r.RuleName, ComplianceRuleCategory.From(r.Category), UtcTimestamp.From(r.EvaluatedAtUtc))
                : ComplianceRuleResult.Fail(ComplianceEvaluationId.FromGuid(r.ComplianceEvaluationId), ComplianceRuleId.FromGuid(r.RuleId), r.RuleCode, r.RuleName, ComplianceRuleCategory.From(r.Category), UtcTimestamp.From(r.EvaluatedAtUtc), violations);

            resultList.Add(child);
        }

        return agg;
    }

    private sealed record ViolationJson(string ViolationCode, string Description, string Severity, string? RemediationGuidance);

    private sealed record EvaluationRow
    {
        public required Guid ComplianceEvaluationId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string TargetEntityType { get; init; }
        public required Guid TargetEntityId { get; init; }
        public required string Status { get; init; }
        public required DateTime StartedAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public required Guid TriggeredBy { get; init; }
    }

    private sealed record RuleResultRow
    {
        public required Guid ComplianceRuleResultId { get; init; }
        public required Guid ComplianceEvaluationId { get; init; }
        public required Guid RuleId { get; init; }
        public required string RuleCode { get; init; }
        public required string RuleName { get; init; }
        public required string Category { get; init; }
        public required bool Passed { get; init; }
        public required string ViolationsJson { get; init; }
        public required DateTime EvaluatedAtUtc { get; init; }
    }
}
