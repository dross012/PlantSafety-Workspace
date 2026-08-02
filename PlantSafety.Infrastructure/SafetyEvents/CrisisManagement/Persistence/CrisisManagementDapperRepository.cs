// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Infrastructure
//  File:           CrisisManagementDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CrisisManagement read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.CrisisManagement.Models;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Repositories;
// CrisisManagementDetailDto lives in the same Models namespace as CrisisManagementItemDto
// (imported above) -- no extra using required for the Phase 4 GetByIdAsync addition.

namespace PlantSafety.Infrastructure.SafetyEvents.CrisisManagement.Persistence;

/// <summary>
/// Dapper-backed repository for CrisisManagement read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class CrisisManagementDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<CrisisManagementDapperRepository> logger)
    : ICrisisManagementRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<CrisisManagementItemDto>> GetCrisisEventsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetCrisisEvents for CrisisManagement tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CrisisManagementItemDto>(
            "[SafetyEvents].[usp_CrisisManagement_GetCrisisEvents]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<CrisisManagementDetailDto?> GetByIdAsync(Guid crisisManagementId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for CrisisManagement {CrisisManagementId} Tenant {TenantId}", crisisManagementId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("CrisisManagementId", crisisManagementId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CrisisManagementDetailDto>(
            "[SafetyEvents].[usp_CrisisManagement_GetById]", parameters, cancellationToken);
    }
}
