// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Infrastructure
//  File:           SafetyPolicyDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of ISafetyPolicyRepository (Domain repository
//                  interface for the SafetyPolicy aggregate root). All DB access via stored
//                  procedures in the [SafetyPolicies] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because SafetyPolicy exposes only a private
//                  "for rehydration only" constructor and private-setter properties.
//                  ListAsync evaluates the supplied SafetyPolicySpecification in-memory over the
//                  full active candidate set, since the Domain-layer specification is an
//                  Expression&lt;Func&lt;SafetyPolicy,bool&gt;&gt; rather than a SQL predicate, and
//                  the interface carries no tenant/site scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyPolicies.Aggregates;
using PlantSafety.Domain.SafetyPolicies.Entities;
using PlantSafety.Domain.SafetyPolicies.Enums;
using PlantSafety.Domain.SafetyPolicies.Events;
using PlantSafety.Domain.SafetyPolicies.Interfaces;
using PlantSafety.Domain.SafetyPolicies.Specifications;
using PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.SafetyPolicies.Persistence;

/// <summary>Dapper-backed repository for the <see cref="SafetyPolicy"/> aggregate root.</summary>
public sealed class SafetyPolicyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<SafetyPolicyDapperRepository> logger)
    : ISafetyPolicyRepository
{
    /// <inheritdoc/>
    public async Task<SafetyPolicy?> GetByIdAsync(SafetyPolicyId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for SafetyPolicy {SafetyPolicyId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("SafetyPolicyId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<SafetyPolicyRow>(
            "[SafetyPolicies].[usp_SafetyPolicy_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        var acks = await GetAcknowledgmentRowsAsync(id, cancellationToken);
        return Hydrate(row, acks);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SafetyPolicy>> ListAsync(SafetyPolicySpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List SafetyPolicy via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<SafetyPolicyRow>(
            "[SafetyPolicies].[usp_SafetyPolicy_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<SafetyPolicy>();

        foreach (var row in rows)
        {
            var acks = await GetAcknowledgmentRowsAsync(SafetyPolicyId.FromGuid(row.SafetyPolicyId), cancellationToken);
            var aggregate = Hydrate(row, acks);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(SafetyPolicy aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add SafetyPolicy {SafetyPolicyId}", aggregate.SafetyPolicyId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SafetyPolicyId", aggregate.SafetyPolicyId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 30)
            .AddString("SafetyPolicyTitle", aggregate.SafetyPolicyTitle, 400)
            .AddString("PolicyNumber", aggregate.PolicyNumber, 100)
            .AddInt("SafetyPolicyVersion", aggregate.SafetyPolicyVersion)
            .AddString("Category", aggregate.Category.Code, 100)
            .AddString("Applicability", aggregate.Applicability, 400)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddGuid("ApprovedBy", aggregate.ApprovedBy?.Value)
            .AddDateTime("ApprovedAtUtc", aggregate.ApprovedAtUtc?.Value)
            .AddDateTime("EffectiveAtUtc", aggregate.EffectiveAtUtc?.Value)
            .AddString("ReviewCadence", aggregate.ReviewCadence.Code, 30)
            .AddDateTime("LastReviewedAtUtc", aggregate.LastReviewedAtUtc?.Value)
            .AddDateTime("NextReviewDueUtc", aggregate.NextReviewDueUtc)
            .AddBool("ReviewRequired", aggregate.ReviewRequired)
            .AddInt("RequiredAcknowledgmentCount", aggregate.RequiredAcknowledgmentCount)
            .AddInt("ReviewCount", aggregate.ReviewCount)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyPolicies].[usp_SafetyPolicy_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    
        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events
        // raised by the aggregate during this write so TransactionalUnitOfWork.CommitAsync
        // (invoked by TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages]. See TransactionalUnitOfWork remarks for the known
        // eventual-consistency gap (outbox flush is not in the same SQL transaction).
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
}

    /// <inheritdoc/>
    public async Task UpdateAsync(SafetyPolicy aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update SafetyPolicy {SafetyPolicyId}", aggregate.SafetyPolicyId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SafetyPolicyId", aggregate.SafetyPolicyId.Value)
            .AddString("Status", aggregate.Status.ToString(), 30)
            .AddInt("SafetyPolicyVersion", aggregate.SafetyPolicyVersion)
            .AddString("Category", aggregate.Category.Code, 100)
            .AddGuid("ApprovedBy", aggregate.ApprovedBy?.Value)
            .AddDateTime("ApprovedAtUtc", aggregate.ApprovedAtUtc?.Value)
            .AddDateTime("EffectiveAtUtc", aggregate.EffectiveAtUtc?.Value)
            .AddString("ReviewCadence", aggregate.ReviewCadence.Code, 30)
            .AddDateTime("LastReviewedAtUtc", aggregate.LastReviewedAtUtc?.Value)
            .AddDateTime("NextReviewDueUtc", aggregate.NextReviewDueUtc)
            .AddInt("ReviewCount", aggregate.ReviewCount)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyPolicies].[usp_SafetyPolicy_Update]", parameters, tx, cancellationToken: cancellationToken);

        // Reconcile the Acknowledgments child collection: soft-delete existing rows, then
        // reinsert the current in-memory collection. Simple and correct for the expected
        // acknowledgment volumes; avoids requiring a TVP merge for this Dapper-only stack.
        var deleteParams = new StoredProcedureBuilder().AddGuid("SafetyPolicyId", aggregate.SafetyPolicyId.Value).Build();
        await commandExecutor.ExecuteAsync(conn, "[SafetyPolicies].[usp_PolicyAcknowledgment_DeleteByPolicyId]", deleteParams, tx, cancellationToken: cancellationToken);

        foreach (var ack in aggregate.Acknowledgments)
        {
            var ackParams = new StoredProcedureBuilder()
                .AddGuid("AcknowledgmentId", ack.AcknowledgmentId)
                .AddGuid("SafetyPolicyId", aggregate.SafetyPolicyId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddInt("AcknowledgedPolicyVersion", ack.AcknowledgedPolicyVersion)
                .AddGuid("UserId", ack.UserId.Value)
                .AddDateTime("AcknowledgedAtUtc", ack.AcknowledgedAtUtc.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[SafetyPolicies].[usp_PolicyAcknowledgment_Insert]", ackParams, tx, cancellationToken: cancellationToken);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    private async Task<IReadOnlyList<PolicyAcknowledgmentRow>> GetAcknowledgmentRowsAsync(SafetyPolicyId id, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("SafetyPolicyId", id.Value).Build();
        return await queryExecutor.QueryAsync<PolicyAcknowledgmentRow>(
            "[SafetyPolicies].[usp_PolicyAcknowledgment_GetByPolicyId]", parameters, cancellationToken);
    }

    /// <summary>Reconstructs a <see cref="SafetyPolicy"/> aggregate from its flat read-model row and child rows.</summary>
    private static SafetyPolicy Hydrate(SafetyPolicyRow row, IReadOnlyList<PolicyAcknowledgmentRow> ackRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<SafetyPolicy>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.SafetyPolicyId), SafetyPolicyId.FromGuid(row.SafetyPolicyId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.Status), Enum.Parse<SafetyPolicyStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.SafetyPolicyTitle), row.SafetyPolicyTitle);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.PolicyNumber), row.PolicyNumber);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.SafetyPolicyVersion), row.SafetyPolicyVersion);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.Category), PolicyCategory.From(row.Category));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.Applicability), row.Applicability);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.OwnerId), new UserId(row.OwnerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.ApprovedBy), row.ApprovedBy.HasValue ? new UserId(row.ApprovedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.ApprovedAtUtc), row.ApprovedAtUtc.HasValue ? UtcTimestamp.From(row.ApprovedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.EffectiveAtUtc), row.EffectiveAtUtc.HasValue ? UtcTimestamp.From(row.EffectiveAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.ReviewCadence), ReviewCadence.From(row.ReviewCadence));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.LastReviewedAtUtc), row.LastReviewedAtUtc.HasValue ? UtcTimestamp.From(row.LastReviewedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.NextReviewDueUtc), row.NextReviewDueUtc);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.ReviewRequired), row.ReviewRequired);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.RequiredAcknowledgmentCount), row.RequiredAcknowledgmentCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.ReviewCount), row.ReviewCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SafetyPolicy.CreatedBy), new UserId(row.CreatedBy));

        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ISafetyPolicyDomainEvent>());
        AggregateReflectionHydrator.SetField(agg, "_acknowledgments", new List<PolicyAcknowledgment>());

        var ackList = AggregateReflectionHydrator.GetPrivateCollectionField<SafetyPolicy, List<PolicyAcknowledgment>>(agg, "_acknowledgments");
        foreach (var a in ackRows)
        {
            ackList.Add(new PolicyAcknowledgment(
                a.AcknowledgmentId,
                SafetyPolicyId.FromGuid(a.SafetyPolicyId),
                a.AcknowledgedPolicyVersion,
                new UserId(a.UserId),
                UtcTimestamp.From(a.AcknowledgedAtUtc)));
        }

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_SafetyPolicy_GetById</c>/<c>usp_SafetyPolicy_GetAllActive</c>.</summary>
    private sealed record SafetyPolicyRow
    {
        public required Guid SafetyPolicyId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string SafetyPolicyTitle { get; init; }
        public required string PolicyNumber { get; init; }
        public required int SafetyPolicyVersion { get; init; }
        public required string Category { get; init; }
        public required string Applicability { get; init; }
        public required Guid OwnerId { get; init; }
        public Guid? ApprovedBy { get; init; }
        public DateTime? ApprovedAtUtc { get; init; }
        public DateTime? EffectiveAtUtc { get; init; }
        public required string ReviewCadence { get; init; }
        public DateTime? LastReviewedAtUtc { get; init; }
        public DateTime? NextReviewDueUtc { get; init; }
        public required bool ReviewRequired { get; init; }
        public required int RequiredAcknowledgmentCount { get; init; }
        public required int ReviewCount { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PolicyAcknowledgment_GetByPolicyId</c>.</summary>
    private sealed record PolicyAcknowledgmentRow
    {
        public required Guid AcknowledgmentId { get; init; }
        public required Guid SafetyPolicyId { get; init; }
        public required int AcknowledgedPolicyVersion { get; init; }
        public required Guid UserId { get; init; }
        public required DateTime AcknowledgedAtUtc { get; init; }
    }
}
