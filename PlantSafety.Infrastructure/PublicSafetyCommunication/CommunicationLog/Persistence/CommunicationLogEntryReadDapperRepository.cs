// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.CommunicationLog
//  Layer:          Infrastructure
//  File:           CommunicationLogEntryReadDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CommunicationLogEntry Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Models;
using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Repositories;

namespace PlantSafety.Infrastructure.PublicSafetyCommunication.CommunicationLog.Persistence;

/// <summary>Dapper-backed repository for CommunicationLogEntry read-model queries.</summary>
public sealed class CommunicationLogEntryReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<CommunicationLogEntryReadDapperRepository> logger)
    : ICommunicationLogEntryRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<CommunicationLogEntryItemDto>> GetCommunicationLogByIncidentAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCommunicationLogByIncident for tenant {TenantId}, incident {IncidentId}", tenantId, incidentId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("IncidentId", incidentId)
            .Build();

        return await queryExecutor.QueryAsync<CommunicationLogEntryItemDto>(
            "[PublicSafetyCommunication].[usp_CommunicationLogEntry_GetByIncident]", parameters, cancellationToken);
    }
}
