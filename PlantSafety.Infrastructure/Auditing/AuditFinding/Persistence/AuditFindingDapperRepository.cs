// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Infrastructure
//  File:           AuditFindingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AuditFinding aggregate root. All DB access via
//                  stored procedures in the [Auditing] schema -- deliberately distinct from
//                  the pre-existing [Compliance].[usp_AuditFindings_GetFindings] read-model
//                  (Infrastructure.Compliance.Auditing.AuditFindings, a different module).
//                  Aggregate rehydration uses AggregateReflectionHydrator because AuditFinding
//                  exposes no public Rehydrate()/factory path. Additive-only evolution.
//
//                  KNOWN LIMITATION: see AuditEngagementDapperRepository -- ListAsync's
//                  specification-only signature has no tenantId, so it fails closed (empty
//                  result) rather than risk a cross-tenant leak.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditFinding.Aggregates;
using PlantSafety.Domain.Auditing.AuditFinding.Enums;
using PlantSafety.Domain.Auditing.AuditFinding.Interfaces;
using PlantSafety.Domain.Auditing.AuditFinding.Specifications;
using PlantSafety.Domain.Auditing.AuditFinding.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Auditing.AuditFinding.Persistence;

/// <summary>
/// Dapper-backed repository for the <see cref="Domain.Auditing.AuditFinding.Aggregates.AuditFinding"/>
/// aggregate. Executes stored procedures for all persistence operations.
/// </summary>
public sealed class AuditFindingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AuditFindingDapperRepository> logger)
    : IAuditFindingRepository
{
    /// <inheritdoc/>
    public async Task<Domain.Auditing.AuditFinding.Aggregates.AuditFinding?> GetByIdAsync(
        AuditFindingId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById AuditFinding {AuditFindingId}", id.Value);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AuditFindingId", id.Value)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<AuditFindingRow>(
            "[Auditing].[usp_AuditFinding_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Domain.Auditing.AuditFinding.Aggregates.AuditFinding>> ListAsync(
        AuditFindingSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List AuditFinding via specification {Specification}", specification.GetType().Name);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", Guid.Empty)
            .AddGuid("SiteId", (Guid?)null)
            .Build();

        var rows = await queryExecutor.QueryAsync<AuditFindingRow>(
            "[Auditing].[usp_AuditFinding_ListByTenant]", parameters, cancellationToken);

        var predicate = specification.ToPredicate();
        return rows.Select(Hydrate).Where(predicate).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(
        Domain.Auditing.AuditFinding.Aggregates.AuditFinding aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Create AuditFinding {AuditFindingId} for tenant {TenantId}", aggregate.AuditFindingId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("AuditFindingId", aggregate.AuditFindingId.Value)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddGuid("AuditEngagementId", aggregate.AuditEngagementId)
            .AddString("FindingTitle", aggregate.FindingTitle, 200)
            .AddString("Severity", aggregate.Severity, 20)
            .AddBool("CorrectiveActionRequired", aggregate.CorrectiveActionRequired)
            .AddGuid("AssignedToUserId", aggregate.AssignedToUserId?.Value)
            .AddDateTime("DueDateUtc", aggregate.DueDateUtc?.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Auditing].[usp_AuditFinding_Create]", parameters, tx, cancellationToken: cancellationToken);
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
        Domain.Auditing.AuditFinding.Aggregates.AuditFinding aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update AuditFinding {AuditFindingId}", aggregate.AuditFindingId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("AuditFindingId", aggregate.AuditFindingId.Value)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("Severity", aggregate.Severity, 20)
            .AddBool("CorrectiveActionRequired", aggregate.CorrectiveActionRequired)
            .AddGuid("AssignedToUserId", aggregate.AssignedToUserId?.Value)
            .AddDateTime("DueDateUtc", aggregate.DueDateUtc?.Value)
            .AddDateTime("VerifiedAtUtc", aggregate.VerifiedAtUtc?.Value)
            .AddString("RemediationNotes", aggregate.RemediationNotes, 2000)
            .AddDateTime("RemediatedAtUtc", aggregate.RemediatedAtUtc?.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Auditing].[usp_AuditFinding_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    private static Domain.Auditing.AuditFinding.Aggregates.AuditFinding Hydrate(AuditFindingRow row)
    {
        var aggregate = AggregateReflectionHydrator.CreateUninitialized<Domain.Auditing.AuditFinding.Aggregates.AuditFinding>();

        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.AuditFindingId), AuditFindingId.FromGuid(row.AuditFindingId));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.Status), Enum.Parse<AuditFindingStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.AuditEngagementId), row.AuditEngagementId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.FindingTitle), row.FindingTitle);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.Severity), row.Severity);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.CorrectiveActionRequired), row.CorrectiveActionRequired);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.AssignedToUserId), row.AssignedToUserId.HasValue ? new UserId(row.AssignedToUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.DueDateUtc), row.DueDateUtc.HasValue ? new UtcTimestamp(row.DueDateUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.VerifiedAtUtc), row.VerifiedAtUtc.HasValue ? new UtcTimestamp(row.VerifiedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.RemediationNotes), row.RemediationNotes);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.RemediatedAtUtc), row.RemediatedAtUtc.HasValue ? new UtcTimestamp(row.RemediatedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.CreatedAtUtc), new UtcTimestamp(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.Auditing.AuditFinding.Aggregates.AuditFinding.CreatedBy), new UserId(row.CreatedBy));

        // Rehydration bug fix (same class of bug proven via SmokeHarness Check 2 against the
        // sibling AuditEngagement aggregate): _domainEvents was never initialized here, so any
        // lifecycle-transition method calling AddDomainEvent() on a rehydrated aggregate would
        // throw NullReferenceException. Mirrors RiskDapperRepository's pattern.
        AggregateReflectionHydrator.SetField(aggregate, "_domainEvents", new List<Domain.Auditing.AuditFinding.Events.IAuditFindingDomainEvent>());

        return aggregate;
    }

    private sealed record AuditFindingRow
    {
        public required Guid AuditFindingId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required Guid AuditEngagementId { get; init; }
        public required string FindingTitle { get; init; }
        public required string Severity { get; init; }
        public required bool CorrectiveActionRequired { get; init; }
        public Guid? AssignedToUserId { get; init; }
        public DateTime? DueDateUtc { get; init; }
        public DateTime? VerifiedAtUtc { get; init; }
        public string? RemediationNotes { get; init; }
        public DateTime? RemediatedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }
}
