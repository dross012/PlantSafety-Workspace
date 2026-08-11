// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification.ISO45001
//  Layer:          Infrastructure
//  File:           ISO45001DapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Certification.ISO45001 read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Certification.ISO45001.Models;
using PlantSafety.Application.Compliance.Certification.ISO45001.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Certification.ISO45001.Persistence;

/// <summary>
/// Dapper-backed repository for Certification.ISO45001 read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ISO45001DapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<ISO45001DapperRepository> logger)
    : IISO45001Repository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ISO45001ItemDto>> GetISO45001Async(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetISO45001 for Certification.ISO45001 tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ISO45001ItemDto>(
            "[Compliance].[usp_CertificationISO45001_GetISO45001]", parameters, cancellationToken);
    }
}
