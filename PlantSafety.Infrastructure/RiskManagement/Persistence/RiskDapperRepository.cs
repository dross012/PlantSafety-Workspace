// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Infrastructure
//  File:           RiskDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IRiskRepository (Domain repository interface
//                  for the Risk aggregate root). All DB access via stored procedures in the
//                  [RiskMgmt] schema. Aggregate reconstruction uses AggregateReflectionHydrator
//                  because Risk exposes only a private "for rehydration only" constructor and
//                  private-setter properties. ListAsync evaluates the supplied RiskSpecification
//                  in-memory over the full active candidate set, since the Domain-layer
//                  specification is an Expression&lt;Func&lt;Risk,bool&gt;&gt; rather than a SQL
//                  predicate, and the interface carries no tenant/site scoping parameter to push
//                  down to SQL.
//
//                  ASSUMPTION (flagged): the 11 RiskManagement category read-model repositories
//                  (ChemicalEnvironmentalSafety, LotoConfinedSpaces, etc.) do not correspond to
//                  the Risk aggregate's own 4-value RiskLevel taxonomy (Low/Medium/High/Critical),
//                  and the Risk aggregate has no other property distinguishing 11 hazard-topic
//                  domains. AddAsync therefore derives a best-effort HazardDomainCategory
//                  classification tag from the aggregate's free-text HazardSource/Location
//                  fields via simple keyword matching (see DeriveHazardDomainCategory below),
//                  purely for Infrastructure-layer read-model routing -- this does not change any
//                  Domain-layer behavior or persisted domain state.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.Domain.RiskManagement.Entities;
using PlantSafety.Domain.RiskManagement.Enums;
using PlantSafety.Domain.RiskManagement.Events;
using PlantSafety.Domain.RiskManagement.Interfaces;
using PlantSafety.Domain.RiskManagement.Specifications;
using PlantSafety.Domain.RiskManagement.ValueObjects;
using PlantSafety.Domain.RiskManagement.ValueObjects.Ids;
using PlantSafety.Infrastructure.Messaging.CrossModule;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.RiskManagement.Persistence;

/// <summary>Dapper-backed repository for the <see cref="Risk"/> aggregate root.</summary>
public sealed class RiskDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<RiskDapperRepository> logger,
    ICrossModuleEventRelay? crossModuleEventRelay = null)
    : IRiskRepository
{
    private static readonly (string Category, string[] Keywords)[] HazardDomainKeywordMap =
    [
        ("ChemicalEnvironmentalSafety", ["chemical", "environment", "spill", "toxin", "emission"]),
        ("LotoConfinedSpaces", ["confined space", "loto", "lockout", "tagout"]),
        ("MachineOperationalGuarding", ["machine", "guard", "pinch point", "moving part"]),
        ("ErgonomicsMaterialHandling", ["ergonomic", "lifting", "material handling", "posture"]),
        ("EmergencyPreparedness", ["emergency", "evacuation", "drill"]),
        ("PreStartupSafetyReview", ["pre-startup", "prestartup", "commissioning"]),
        ("ProcessSafetyInformation", ["process safety", "psi", "pressure", "temperature"]),
        ("TrainingContractorManagement", ["contractor", "training gap", "competency"]),
        ("ControlMeasuresHierarchy", ["control measure", "hierarchy of controls"]),
        ("ContinuousMonitoringReview", ["monitoring", "periodic review", "audit"]),
    ];

    /// <inheritdoc/>
    public async Task<Risk?> GetByIdAsync(RiskId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Risk {RiskId} Tenant {TenantId}", id.Value, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("RiskId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<RiskRow>(
            "[RiskMgmt].[usp_Risk_GetById]", parameters, cancellationToken);

        if (row is null)
            return null;

        var cmRows = await GetControlMeasureRowsAsync(id, cancellationToken);
        return Hydrate(row, cmRows);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Risk>> ListAsync(Guid tenantId, RiskSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List Risk via specification {Specification} for Tenant {TenantId}", specification.GetType().Name, tenantId);

        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<RiskRow>(
            "[RiskMgmt].[usp_Risk_GetAllActive]", parameters, cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<Risk>();

        foreach (var row in rows)
        {
            var cmRows = await GetControlMeasureRowsAsync(RiskId.FromGuid(row.RiskId), cancellationToken);
            var aggregate = Hydrate(row, cmRows);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(Risk aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Risk {RiskId}", aggregate.RiskId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var hazardDomainCategory = DeriveHazardDomainCategory(aggregate.HazardSource, aggregate.Location);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RiskId", aggregate.RiskId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 30)
            .AddString("RiskTitle", aggregate.RiskTitle, 400)
            .AddString("Category", aggregate.Category.Code, 30)
            .AddString("HazardDomainCategory", hazardDomainCategory, 100)
            .AddString("Location", aggregate.Location, 400)
            .AddString("HazardSource", aggregate.HazardSource, 400)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddInt("CurrentSeverity", aggregate.CurrentScore?.Severity)
            .AddInt("CurrentLikelihood", aggregate.CurrentScore?.Likelihood)
            .AddString("Priority", aggregate.Priority?.Code, 10)
            .AddDateTime("DocumentedAtUtc", aggregate.DocumentedAtUtc?.Value)
            .AddInt("ResidualSeverity", aggregate.ResidualScore?.Severity)
            .AddInt("ResidualLikelihood", aggregate.ResidualScore?.Likelihood)
            .AddDateTime("LastValidatedAtUtc", aggregate.LastValidatedAtUtc?.Value)
            .AddString("ReviewCadence", aggregate.ReviewCadence.Code, 30)
            .AddDateTime("LastReviewedAtUtc", aggregate.LastReviewedAtUtc?.Value)
            .AddDateTime("NextReviewDueUtc", aggregate.NextReviewDueUtc)
            .AddBool("ReviewRequired", aggregate.ReviewRequired)
            .AddInt("ReviewCount", aggregate.ReviewCount)
            .AddInt("RiskVersion", aggregate.RiskVersion)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[RiskMgmt].[usp_Risk_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events
        // raised by the aggregate during this write so TransactionalUnitOfWork.CommitAsync
        // (invoked by TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages]. See TransactionalUnitOfWork remarks for the known
        // eventual-consistency gap (outbox flush is not in the same SQL transaction).
        await RelayDomainEventsAsync(aggregate, cancellationToken);
}

    /// <inheritdoc/>
    public async Task UpdateAsync(Risk aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Risk {RiskId}", aggregate.RiskId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("RiskId", aggregate.RiskId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 30)
            .AddString("Category", aggregate.Category.Code, 30)
            .AddInt("CurrentSeverity", aggregate.CurrentScore?.Severity)
            .AddInt("CurrentLikelihood", aggregate.CurrentScore?.Likelihood)
            .AddString("Priority", aggregate.Priority?.Code, 10)
            .AddDateTime("DocumentedAtUtc", aggregate.DocumentedAtUtc?.Value)
            .AddInt("ResidualSeverity", aggregate.ResidualScore?.Severity)
            .AddInt("ResidualLikelihood", aggregate.ResidualScore?.Likelihood)
            .AddDateTime("LastValidatedAtUtc", aggregate.LastValidatedAtUtc?.Value)
            .AddString("ReviewCadence", aggregate.ReviewCadence.Code, 30)
            .AddDateTime("LastReviewedAtUtc", aggregate.LastReviewedAtUtc?.Value)
            .AddDateTime("NextReviewDueUtc", aggregate.NextReviewDueUtc)
            .AddInt("ReviewCount", aggregate.ReviewCount)
            .AddInt("RiskVersion", aggregate.RiskVersion)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable the
        // @ETag concurrency check below. See usp_Risk_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[RiskMgmt].[usp_Risk_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Risk {aggregate.RiskId.Value} was modified by another process or the ETag is stale.", Guid.Empty);
        }

        // Reconcile ControlMeasures: entries are immutable once applied -- upsert each.
        foreach (var cm in aggregate.ControlMeasures)
        {
            var cmParams = new StoredProcedureBuilder()
                .AddGuid("ControlMeasureId", cm.ControlMeasureId.Value)
                .AddGuid("RiskId", aggregate.RiskId.Value)
                .AddGuid("TenantId", aggregate.TenantId)
                .AddString("HierarchyLevel", cm.HierarchyLevel.Code, 30)
                .AddString("Description", cm.Description, 2000)
                .AddString("Justification", cm.Justification, 2000)
                .AddGuid("ResponsiblePersonId", cm.ResponsiblePersonId.Value)
                .AddDateTime("ImplementedAtUtc", cm.ImplementedAtUtc.Value)
                .Build();

            await commandExecutor.ExecuteAsync(conn, "[RiskMgmt].[usp_ControlMeasure_Upsert]", cmParams, tx, cancellationToken: cancellationToken);
        }

        tx.Commit();

        await RelayDomainEventsAsync(aggregate, cancellationToken);
    }

    /// <summary>
    /// Enqueues the aggregate's pending domain events to both the intra-platform outbox and (for
    /// the curated subset with a registered cross-module DTO) Shared.EventBus, then flushes the
    /// cross-module relay now that the SQL transaction above has already committed
    /// (Publish-After-Commit; mirrors UnitOfWork.CommitAsync's discipline). This repository owns
    /// its own connection/transaction rather than going through TransactionBehavior/UnitOfWork, so
    /// it must drive the relay itself.
    /// </summary>
    private async Task RelayDomainEventsAsync(Risk aggregate, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
            crossModuleEventRelay?.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();

        if (crossModuleEventRelay is { HasPendingEvents: true })
            await crossModuleEventRelay.FlushAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ControlMeasureRow>> GetControlMeasureRowsAsync(RiskId id, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("RiskId", id.Value).Build();
        return await queryExecutor.QueryAsync<ControlMeasureRow>(
            "[RiskMgmt].[usp_ControlMeasure_GetByRiskId]", parameters, cancellationToken);
    }

    /// <summary>
    /// Best-effort keyword-based classification of a Risk's free-text HazardSource/Location into
    /// one of the 11 RiskManagement read-model hazard-topic categories. Falls back to
    /// "HazardIdentificationMeasurement" (the Phase-1 default step) when no keyword matches.
    /// Infrastructure-layer read-model routing only -- see class-level remarks.
    /// </summary>
    private static string DeriveHazardDomainCategory(string hazardSource, string location)
    {
        var haystack = $"{hazardSource} {location}".ToLowerInvariant();

        foreach (var (category, keywords) in HazardDomainKeywordMap)
            if (keywords.Any(k => haystack.Contains(k, StringComparison.Ordinal)))
                return category;

        return "HazardIdentificationMeasurement";
    }

    /// <summary>Reconstructs a <see cref="Risk"/> aggregate from its flat read-model row and child rows.</summary>
    private static Risk Hydrate(RiskRow row, IReadOnlyList<ControlMeasureRow> cmRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<Risk>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.RiskId), RiskId.FromGuid(row.RiskId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.Status), Enum.Parse<RiskStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.RiskTitle), row.RiskTitle);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.Category), RiskLevel.From(row.Category));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.Location), row.Location);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.HazardSource), row.HazardSource);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.OwnerId), new UserId(row.OwnerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.CurrentScore),
            row.CurrentSeverity.HasValue && row.CurrentLikelihood.HasValue ? new RiskScore(row.CurrentSeverity.Value, row.CurrentLikelihood.Value) : (RiskScore?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.Priority), row.Priority is null ? null : RiskPriority.From(row.Priority));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.DocumentedAtUtc), row.DocumentedAtUtc.HasValue ? UtcTimestamp.From(row.DocumentedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.ResidualScore),
            row.ResidualSeverity.HasValue && row.ResidualLikelihood.HasValue ? new RiskScore(row.ResidualSeverity.Value, row.ResidualLikelihood.Value) : (RiskScore?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.LastValidatedAtUtc), row.LastValidatedAtUtc.HasValue ? UtcTimestamp.From(row.LastValidatedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.ReviewCadence), RiskReviewCadence.From(row.ReviewCadence));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.LastReviewedAtUtc), row.LastReviewedAtUtc.HasValue ? UtcTimestamp.From(row.LastReviewedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.NextReviewDueUtc), row.NextReviewDueUtc);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.ReviewRequired), row.ReviewRequired);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.ReviewCount), row.ReviewCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.RiskVersion), row.RiskVersion);
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(Risk.CreatedBy), new UserId(row.CreatedBy));

        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IRiskDomainEvent>());
        AggregateReflectionHydrator.SetField(agg, "_controlMeasures", new List<ControlMeasure>());

        var cmList = AggregateReflectionHydrator.GetPrivateCollectionField<Risk, List<ControlMeasure>>(agg, "_controlMeasures");
        foreach (var c in cmRows)
        {
            var child = AggregateReflectionHydrator.CreateUninitialized<ControlMeasure>();
            AggregateReflectionHydrator.SetProperty(child, nameof(ControlMeasure.ControlMeasureId), ControlMeasureId.FromGuid(c.ControlMeasureId));
            AggregateReflectionHydrator.SetProperty(child, nameof(ControlMeasure.RiskId), RiskId.FromGuid(c.RiskId));
            AggregateReflectionHydrator.SetProperty(child, nameof(ControlMeasure.HierarchyLevel), ControlHierarchyLevel.From(c.HierarchyLevel));
            AggregateReflectionHydrator.SetProperty(child, nameof(ControlMeasure.Description), c.Description);
            AggregateReflectionHydrator.SetProperty(child, nameof(ControlMeasure.Justification), c.Justification);
            AggregateReflectionHydrator.SetProperty(child, nameof(ControlMeasure.ResponsiblePersonId), new UserId(c.ResponsiblePersonId));
            AggregateReflectionHydrator.SetProperty(child, nameof(ControlMeasure.ImplementedAtUtc), UtcTimestamp.From(c.ImplementedAtUtc));

            cmList.Add(child);
        }

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_Risk_GetById</c>/<c>usp_Risk_GetAllActive</c>.</summary>
    private sealed record RiskRow
    {
        public required Guid RiskId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string RiskTitle { get; init; }
        public required string Category { get; init; }
        public required string Location { get; init; }
        public required string HazardSource { get; init; }
        public required Guid OwnerId { get; init; }
        public int? CurrentSeverity { get; init; }
        public int? CurrentLikelihood { get; init; }
        public string? Priority { get; init; }
        public DateTime? DocumentedAtUtc { get; init; }
        public int? ResidualSeverity { get; init; }
        public int? ResidualLikelihood { get; init; }
        public DateTime? LastValidatedAtUtc { get; init; }
        public required string ReviewCadence { get; init; }
        public DateTime? LastReviewedAtUtc { get; init; }
        public DateTime? NextReviewDueUtc { get; init; }
        public required bool ReviewRequired { get; init; }
        public required int ReviewCount { get; init; }
        public required int RiskVersion { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }

        /// <summary>
        /// Optimistic-concurrency token, projected from the row's native RowVersion column
        /// (<c>CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)</c>). Not yet consumed by
        /// any write path -- the <see cref="Risk"/> aggregate has no ETag concept and
        /// usp_Risk_Update does not take an @ETag parameter -- present so a future
        /// optimistic-concurrency pass has it available without another read-model change,
        /// matching the schema-only precedent set by AccidentManagement's AccidentRow.ETag.
        /// </summary>
        public string? ETag { get; init; }
    }

    /// <summary>Flat read-model row shape returned by <c>usp_ControlMeasure_GetByRiskId</c>.</summary>
    private sealed record ControlMeasureRow
    {
        public required Guid ControlMeasureId { get; init; }
        public required Guid RiskId { get; init; }
        public required string HierarchyLevel { get; init; }
        public required string Description { get; init; }
        public string? Justification { get; init; }
        public required Guid ResponsiblePersonId { get; init; }
        public required DateTime ImplementedAtUtc { get; init; }

        /// <summary>See <see cref="RiskRow.ETag"/> remarks -- not yet consumed by any write path.</summary>
        public string? ETag { get; init; }
    }
}
