// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Infrastructure
//  File:           IncidentManagementRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for IncidentManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.SafetyEvents.IncidentManagement.Persistence;

/// <summary>Dapper-backed repository for <see cref="IncidentManagementReadModel"/> queries.</summary>
public sealed class IncidentManagementRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<IncidentManagementRepository> logger)
{
    /// <summary>Returns the IncidentManagement read model or <c>null</c> if not found.</summary>
    public async Task<IncidentManagementReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for IncidentManagement {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("IncidentManagementId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<IncidentManagementReadModel>(
            conn, $"[SafetyEvents].[usp_IncidentManagement_GetById]", parameters);
    }

    /// <summary>Returns all IncidentManagement records for the tenant.</summary>
    public async Task<IReadOnlyList<IncidentManagementReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for IncidentManagement tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<IncidentManagementReadModel>(
            conn, $"[SafetyEvents].[usp_IncidentManagement_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
