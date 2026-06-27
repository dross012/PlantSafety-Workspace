// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Infrastructure
//  File:           EmergencyManagementRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for EmergencyManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.SafetyEvents.EmergencyManagement.Persistence;

/// <summary>Dapper-backed repository for <see cref="EmergencyManagementReadModel"/> queries.</summary>
public sealed class EmergencyManagementRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EmergencyManagementRepository> logger)
{
    /// <summary>Returns the EmergencyManagement read model or <c>null</c> if not found.</summary>
    public async Task<EmergencyManagementReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for EmergencyManagement {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("EmergencyManagementId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<EmergencyManagementReadModel>(
            conn, $"[SafetyEvents].[usp_EmergencyManagement_GetById]", parameters);
    }

    /// <summary>Returns all EmergencyManagement records for the tenant.</summary>
    public async Task<IReadOnlyList<EmergencyManagementReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for EmergencyManagement tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<EmergencyManagementReadModel>(
            conn, $"[SafetyEvents].[usp_EmergencyManagement_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
