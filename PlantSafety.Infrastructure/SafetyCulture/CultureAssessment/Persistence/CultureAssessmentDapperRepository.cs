// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment
//  Layer:          Infrastructure
//  File:           CultureAssessmentDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CultureAssessment aggregate root. All DB access
//                  via stored procedures in the [SafetyCulture] schema -- deliberately
//                  distinct from the pre-existing [Analytics] schema used by
//                  Infrastructure.Analytics.Culture (a different module: cross-module safety
//                  culture index rollups). Aggregate rehydration uses
//                  AggregateReflectionHydrator because CultureAssessment exposes no public
//                  Rehydrate()/factory path. Additive-only evolution.
//
//                  KNOWN LIMITATION: see AuditEngagementDapperRepository -- ListAsync's
//                  specification-only signature has no tenantId, so it fails closed (empty
//                  result) rather than risk a cross-tenant leak.
// ============================================================================
using PlantSafety.Domain.SafetyCulture.CultureAssessment.Aggregates;
using PlantSafety.Domain.SafetyCulture.CultureAssessment.Enums;
using PlantSafety.Domain.SafetyCulture.CultureAssessment.Interfaces;
using PlantSafety.Domain.SafetyCulture.CultureAssessment.Specifications;
using PlantSafety.Domain.SafetyCulture.CultureAssessment.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.Persistence;

/// <summary>
/// Dapper-backed repository for the <see cref="Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment"/>
/// aggregate. Executes stored procedures for all persistence operations.
/// </summary>
public sealed class CultureAssessmentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CultureAssessmentDapperRepository> logger)
    : ICultureAssessmentRepository
{
    /// <inheritdoc/>
    public async Task<Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment?> GetByIdAsync(
        CultureAssessmentId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById CultureAssessment {CultureAssessmentId}", id.Value);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CultureAssessmentId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<CultureAssessmentRow>(
            "[SafetyCulture].[usp_CultureAssessment_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment>> ListAsync(
        CultureAssessmentSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List CultureAssessment via specification {Specification}", specification.GetType().Name);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", Guid.Empty)
            .AddGuid("SiteId", (Guid?)null)
            .Build();

        var rows = await queryExecutor.QueryAsync<CultureAssessmentRow>(
            "[SafetyCulture].[usp_CultureAssessment_ListByTenant]", parameters, cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(Hydrate).Where(predicate).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(
        Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Create CultureAssessment {CultureAssessmentId} for tenant {TenantId}", aggregate.CultureAssessmentId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("CultureAssessmentId", aggregate.CultureAssessmentId.Value)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("AssessmentTitle", aggregate.AssessmentTitle, 200)
            .AddDateTime("PeriodStartUtc", aggregate.PeriodStartUtc.Value)
            .AddDateTime("PeriodEndUtc", aggregate.PeriodEndUtc.Value)
            .AddDecimal("OverallCultureScore", aggregate.OverallCultureScore)
            .AddDecimal("LeadershipEngagementScore", aggregate.LeadershipEngagementScore)
            .AddString("Summary", aggregate.Summary, 2000)
            .AddGuid("RelatedRiskManagementHazardId", aggregate.RelatedRiskManagementHazardId)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyCulture].[usp_CultureAssessment_Create]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(
        Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update CultureAssessment {CultureAssessmentId}", aggregate.CultureAssessmentId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("CultureAssessmentId", aggregate.CultureAssessmentId.Value)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddDecimal("OverallCultureScore", aggregate.OverallCultureScore)
            .AddDecimal("LeadershipEngagementScore", aggregate.LeadershipEngagementScore)
            .AddString("Summary", aggregate.Summary, 2000)
            .AddDateTime("PublishedAtUtc", aggregate.PublishedAtUtc?.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyCulture].[usp_CultureAssessment_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    private static Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment Hydrate(CultureAssessmentRow row)
    {
        var aggregate = AggregateReflectionHydrator.CreateUninitialized<Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment>();

        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.CultureAssessmentId), CultureAssessmentId.FromGuid(row.CultureAssessmentId));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.Status), Enum.Parse<CultureAssessmentStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.AssessmentTitle), row.AssessmentTitle);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.PeriodStartUtc), new UtcTimestamp(row.PeriodStartUtc));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.PeriodEndUtc), new UtcTimestamp(row.PeriodEndUtc));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.OverallCultureScore), row.OverallCultureScore);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.LeadershipEngagementScore), row.LeadershipEngagementScore);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.Summary), row.Summary);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.RelatedRiskManagementHazardId), row.RelatedRiskManagementHazardId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.CreatedAtUtc), new UtcTimestamp(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyCulture.CultureAssessment.Aggregates.CultureAssessment.PublishedAtUtc), row.PublishedAtUtc.HasValue ? new UtcTimestamp(row.PublishedAtUtc.Value) : (UtcTimestamp?)null);

        // Rehydration bug fix (same class of bug proven via SmokeHarness Check 2 against the
        // sibling Auditing.AuditEngagement aggregate): _domainEvents was never initialized here,
        // so any lifecycle-transition method calling AddDomainEvent() on a rehydrated aggregate
        // would throw NullReferenceException. Mirrors RiskDapperRepository's pattern.
        AggregateReflectionHydrator.SetField(aggregate, "_domainEvents", new List<Domain.SafetyCulture.CultureAssessment.Events.ICultureAssessmentDomainEvent>());

        return aggregate;
    }

    private sealed record CultureAssessmentRow
    {
        public required Guid CultureAssessmentId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string AssessmentTitle { get; init; }
        public required DateTime PeriodStartUtc { get; init; }
        public required DateTime PeriodEndUtc { get; init; }
        public decimal? OverallCultureScore { get; init; }
        public decimal? LeadershipEngagementScore { get; init; }
        public required string Summary { get; init; }
        public Guid? RelatedRiskManagementHazardId { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? PublishedAtUtc { get; init; }
    }
}
