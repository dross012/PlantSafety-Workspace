// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification.ISO14001
//  Layer:          Infrastructure
//  File:           ISO14001DapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Certification.ISO14001 read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Certification.ISO14001.Models;
using PlantSafety.Application.Compliance.Certification.ISO14001.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Certification.ISO14001.Persistence;

/// <summary>
/// Dapper-backed repository for Certification.ISO14001 read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ISO14001DapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ISO14001DapperRepository> logger)
    : IISO14001Repository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ISO14001ItemDto>> GetISO14001Async(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetISO14001 for Certification.ISO14001 tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ISO14001ItemDto>(
            "[Compliance].[usp_CertificationISO14001_GetISO14001]", parameters, cancellationToken);
    }
}
