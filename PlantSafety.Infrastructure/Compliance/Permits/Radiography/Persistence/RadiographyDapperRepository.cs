// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.Radiography
//  Layer:          Infrastructure
//  File:           RadiographyDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.Radiography read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.Radiography.Models;
using PlantSafety.Application.Compliance.Permits.Radiography.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.Radiography.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.Radiography read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class RadiographyDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RadiographyDapperRepository> logger)
    : IRadiographyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<RadiographyItemDto>> GetRadiographyAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRadiography for Permits.Radiography tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RadiographyItemDto>(
            "[Compliance].[usp_PermitRadiography_GetRadiography]", parameters, cancellationToken);
    }
}
