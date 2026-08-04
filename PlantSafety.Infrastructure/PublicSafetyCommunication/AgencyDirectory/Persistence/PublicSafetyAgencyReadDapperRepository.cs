// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Infrastructure
//  File:           PublicSafetyAgencyReadDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PublicSafetyAgency Application-layer read-model.
//                  All DB access via stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Models;
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Repositories;

namespace PlantSafety.Infrastructure.PublicSafetyCommunication.AgencyDirectory.Persistence;

/// <summary>Dapper-backed repository for PublicSafetyAgency read-model queries.</summary>
public sealed class PublicSafetyAgencyReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PublicSafetyAgencyReadDapperRepository> logger)
    : IPublicSafetyAgencyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PublicSafetyAgencyItemDto>> ListPublicSafetyAgenciesAsync(
        Guid tenantId,
        Guid? siteId,
        string? agencyType,
        bool activeOnly,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("ListPublicSafetyAgencies for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("AgencyType", agencyType, 64)
            .AddBool("ActiveOnly", activeOnly)
            .Build();

        return await queryExecutor.QueryAsync<PublicSafetyAgencyItemDto>(
            "[PublicSafetyCommunication].[usp_PublicSafetyAgency_ListByTenant]", parameters, cancellationToken);
    }
}
