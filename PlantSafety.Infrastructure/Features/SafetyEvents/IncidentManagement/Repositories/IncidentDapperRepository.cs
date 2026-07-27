// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.SafetyEvents.IncidentManagement
//  Layer:          Infrastructure
//  File:           IncidentDapperRepository.cs
//  Created:        2026-07-12
//  Updated:        2026-07-27 -- CreateAsync/UpdateAsync now take the Domain IncidentManagement
//                  aggregate (built/rehydrated by CreateIncidentCommandHandler/
//                  UpdateIncidentCommandHandler) instead of raw primitives. SQL parameters are
//                  translated from the aggregate's validated state (Create) or the original
//                  null-means-unchanged diff (Update, preserving the existing ISNULL/COALESCE
//                  partial-update stored procedure -- no full-column overwrite). After a
//                  successful write, the aggregate's pending domain events are enqueued to the
//                  outbox (mirrors AccidentDapperRepository.AddAsync/UpdateAsync) so
//                  TransactionalUnitOfWork.CommitAsync flushes them post-handler.
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Dapper write-side repository for the Incident aggregate. All DB access via
//                  stored procedures under the [SafetyEvents] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using IncidentAggregate = PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates.IncidentManagement;

namespace PlantSafety.Infrastructure.Features.SafetyEvents.IncidentManagement.Repositories;

/// <summary>
/// Dapper-backed write-side repository for the <c>Incident</c> aggregate.
/// Executes stored procedures for all persistence operations within an open UoW transaction.
/// </summary>
public sealed class IncidentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<IncidentDapperRepository> logger)
    : IIncidentRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        IncidentAggregate aggregate,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug(
            "Create Incident {IncidentId} for tenant {TenantId}",
            aggregate.IncidentManagementId.Value, aggregate.TenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, aggregate.CreatedAtUtc.Value)
            .AddGuid("IncidentId", aggregate.IncidentManagementId.Value)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Title", aggregate.Title)
            .AddString("Description", aggregate.Description)
            .AddDateTime("IncidentOccurredAtUtc", aggregate.IncidentOccurredAtUtc.Value)
            .AddString("Severity", aggregate.Severity)
            .AddString("Location", aggregate.Location)
            .AddGuid("ReportedByWorkerId", aggregate.ReportedByWorkerId.Value)
            .AddGuid("AssignedInvestigatorId", aggregate.AssignedInvestigatorId?.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_Incident_Create]", parameters, tx);
        tx.Commit();

        EnqueuePendingEvents(aggregate, actorId, correlationId);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(
        IncidentAggregate aggregate,
        string? title,
        string? description,
        string? severity,
        string? location,
        Guid? assignedInvestigatorId,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var incidentId = aggregate.IncidentManagementId.Value;
        var tenantId = aggregate.TenantId;

        logger.LogDebug("Update Incident {IncidentId} for tenant {TenantId}", incidentId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("IncidentId", incidentId)
            .AddString("Title", title)
            .AddString("Description", description)
            .AddString("Severity", severity)
            .AddString("Location", location)
            .AddGuid("AssignedInvestigatorId", assignedInvestigatorId)
            .AddString("ETag", eTag, size: 64)
            .Build();

        var rowsAffected = await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_Incident_Update]", parameters, tx);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Incident {incidentId} was modified by another process or the ETag is stale.", correlationId);
        }

        tx.Commit();

        EnqueuePendingEvents(aggregate, actorId, correlationId);
    }

    /// <summary>
    /// Enqueues the aggregate's pending domain events (raised by <c>Create</c>/<c>UpdateDetails</c>)
    /// to the outbox after a successful SQL commit, then clears them from the in-memory aggregate.
    /// Mirrors <c>AccidentDapperRepository.AddAsync</c>/<c>UpdateAsync</c>.
    /// </summary>
    private void EnqueuePendingEvents(IncidentAggregate aggregate, Guid actorId, Guid correlationId)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), actorId.ToString(), correlationId.ToString());
        }

        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task SoftDeleteAsync(
        Guid tenantId,
        Guid incidentId,
        string deletionReason,
        string eTag,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Soft-delete Incident {IncidentId} for tenant {TenantId}", incidentId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("IncidentId", incidentId)
            .AddString("DeletionReason", deletionReason)
            .AddString("ETag", eTag, size: 64)
            .Build();

        var rowsAffected = await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_Incident_SoftDelete]", parameters, tx);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Incident {incidentId} was modified by another process or the ETag is stale.", correlationId);
        }

        tx.Commit();
    }
}
