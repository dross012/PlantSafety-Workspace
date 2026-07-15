// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Infrastructure
//  File:           EmergencyManagementDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EmergencyManagement read-model backing
//                  IEmergencyManagementRepository (the legacy SafetyEvents.EmergencyManagement.Repositories
//                  interface). Distinct from the pre-existing EmergencyManagementRepository in this same
//                  folder, which backs the unrelated Features-tree read model.
//                  All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Models;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Repositories;

namespace PlantSafety.Infrastructure.SafetyEvents.EmergencyManagement.Persistence;

/// <summary>
/// Dapper-backed repository for EmergencyManagement read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class EmergencyManagementDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EmergencyManagementDapperRepository> logger)
    : IEmergencyManagementRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmergencyManagementItemDto>> GetEmergenciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEmergencies for EmergencyManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EmergencyManagementItemDto>(
            "[SafetyEvents].[usp_EmergencyManagementItems_GetEmergencies]", parameters, cancellationToken);
    }
}
