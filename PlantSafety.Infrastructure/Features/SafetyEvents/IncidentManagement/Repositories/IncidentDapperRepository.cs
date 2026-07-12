// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.SafetyEvents.IncidentManagement
//  Layer:          Infrastructure
//  File:           IncidentDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper write-side repository for the Incident aggregate. All DB access via
//                  stored procedures under the [SafetyEvents] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

namespace PlantSafety.Infrastructure.Features.SafetyEvents.IncidentManagement.Repositories;

/// <summary>
/// Dapper-backed write-side repository for the <c>Incident</c> aggregate.
/// Executes stored procedures for all persistence operations within an open UoW transaction.
/// </summary>
public sealed class IncidentDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<IncidentDapperRepository> logger)
    : IIncidentRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid incidentId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        DateTimeOffset incidentOccurredAtUtc,
        string severity,
        string location,
        Guid reportedByWorkerId,
        Guid? assignedInvestigatorId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create Incident {IncidentId} for tenant {TenantId}", incidentId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("IncidentId", incidentId)
            .AddGuid("SiteId", siteId)
            .AddString("Title", title)
            .AddString("Description", description)
            .AddDateTime("IncidentOccurredAtUtc", incidentOccurredAtUtc.UtcDateTime)
            .AddString("Severity", severity)
            .AddString("Location", location)
            .AddGuid("ReportedByWorkerId", reportedByWorkerId)
            .AddGuid("AssignedInvestigatorId", assignedInvestigatorId)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_Incident_Create]", parameters, tx);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(
        Guid tenantId,
        Guid incidentId,
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
