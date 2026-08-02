// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Infrastructure
//  File:           AuditEngagementDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AuditEngagement aggregate root. All DB access
//                  via stored procedures in the [Auditing] schema -- deliberately distinct
//                  from the pre-existing [Compliance] schema used by
//                  Infrastructure.Compliance.Auditing (a different module: application/
//                  software audit read-models). Aggregate rehydration uses
//                  AggregateReflectionHydrator because AuditEngagement exposes no public
//                  Rehydrate()/factory path. Additive-only evolution.
//
//                  KNOWN LIMITATION (flagged, not silently absorbed): see
//                  ChecklistCompletionDapperRepository -- ListAsync receives only a
//                  specification (no explicit tenantId on the Domain interface), so it is
//                  implemented here as a fails-closed (empty) candidate fetch scoped to
//                  Guid.Empty; a fully tenant-scoped ListAsync would require extending
//                  IAuditEngagementRepository, out of scope (Domain interfaces unchanged).
// ============================================================================
using PlantSafety.Domain.Auditing.AuditEngagement.Aggregates;
using PlantSafety.Domain.Auditing.AuditEngagement.Enums;
using PlantSafety.Domain.Auditing.AuditEngagement.Interfaces;
using PlantSafety.Domain.Auditing.AuditEngagement.Specifications;
using PlantSafety.Domain.Auditing.AuditEngagement.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Auditing.AuditEngagement.Persistence;

/// <summary>
/// Dapper-backed repository for the <see cref="Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement"/>
/// aggregate. Executes stored procedures for all persistence operations.
/// </summary>
public sealed class AuditEngagementDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AuditEngagementDapperRepository> logger)
    : IAuditEngagementRepository
{
    /// <inheritdoc/>
    public async Task<Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement?> GetByIdAsync(
        AuditEngagementId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById AuditEngagement {AuditEngagementId}", id.Value);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AuditEngagementId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<AuditEngagementRow>(
            "[Auditing].[usp_AuditEngagement_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement>> ListAsync(
        AuditEngagementSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List AuditEngagement via specification {Specification}", specification.GetType().Name);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", Guid.Empty)
            .AddGuid("SiteId", (Guid?)null)
            .Build();

        var rows = await queryExecutor.QueryAsync<AuditEngagementRow>(
            "[Auditing].[usp_AuditEngagement_ListByTenant]", parameters, cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(Hydrate).Where(predicate).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(
        Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Create AuditEngagement {AuditEngagementId} for tenant {TenantId}", aggregate.AuditEngagementId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("AuditEngagementId", aggregate.AuditEngagementId.Value)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("EngagementTitle", aggregate.EngagementTitle, 200)
            .AddString("DisciplineType", aggregate.DisciplineType, 50)
            .AddString("Scope", aggregate.Scope, 2000)
            .AddGuid("LeadAuditorId", aggregate.LeadAuditorId.Value == Guid.Empty ? (Guid?)null : aggregate.LeadAuditorId.Value)
            .AddDateTime("ScheduledAtUtc", aggregate.ScheduledAtUtc?.Value)
            .AddInt("FindingsCount", aggregate.FindingsCount)
            .AddDecimal("ComplianceScore", aggregate.ComplianceScore)
            .AddString("ComplianceRating", aggregate.ComplianceRating, 30)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Auditing].[usp_AuditEngagement_Create]", parameters, tx, cancellationToken: cancellationToken);
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
        Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update AuditEngagement {AuditEngagementId}", aggregate.AuditEngagementId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("AuditEngagementId", aggregate.AuditEngagementId.Value)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddGuid("LeadAuditorId", aggregate.LeadAuditorId.Value == Guid.Empty ? (Guid?)null : aggregate.LeadAuditorId.Value)
            .AddDateTime("ScheduledAtUtc", aggregate.ScheduledAtUtc?.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddInt("FindingsCount", aggregate.FindingsCount)
            .AddDecimal("ComplianceScore", aggregate.ComplianceScore)
            .AddString("ComplianceRating", aggregate.ComplianceRating, 30)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Auditing].[usp_AuditEngagement_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    private static Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement Hydrate(AuditEngagementRow row)
    {
        var aggregate = AggregateReflectionHydrator.CreateUninitialized<Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement>();

        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.AuditEngagementId), AuditEngagementId.FromGuid(row.AuditEngagementId));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.Status), Enum.Parse<AuditEngagementStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.EngagementTitle), row.EngagementTitle);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.DisciplineType), row.DisciplineType);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.Scope), row.Scope);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.LeadAuditorId), row.LeadAuditorId.HasValue ? new UserId(row.LeadAuditorId.Value) : default);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.ScheduledAtUtc), row.ScheduledAtUtc.HasValue ? new UtcTimestamp(row.ScheduledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.CompletedAtUtc), row.CompletedAtUtc.HasValue ? new UtcTimestamp(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.FindingsCount), row.FindingsCount);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.ComplianceScore), row.ComplianceScore);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.ComplianceRating), row.ComplianceRating);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.CreatedAtUtc), new UtcTimestamp(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditEngagement.Aggregates.AuditEngagement.CreatedBy), new UserId(row.CreatedBy));

        // Rehydration bug fix (SmokeHarness Check 2): _domainEvents was never initialized here,
        // so any lifecycle-transition method calling AddDomainEvent() on a rehydrated aggregate
        // (e.g. Schedule()) threw NullReferenceException. Mirrors RiskDapperRepository's pattern.
        AggregateReflectionHydrator.SetField(aggregate, "_domainEvents", new List<Domain.Auditing.AuditEngagement.Events.IAuditEngagementDomainEvent>());

        return aggregate;
    }

    private sealed record AuditEngagementRow
    {
        public required Guid AuditEngagementId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string EngagementTitle { get; init; }
        public required string DisciplineType { get; init; }
        public required string Scope { get; init; }
        public Guid? LeadAuditorId { get; init; }
        public DateTime? ScheduledAtUtc { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public required int FindingsCount { get; init; }
        public decimal? ComplianceScore { get; init; }
        public required string ComplianceRating { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }
}
