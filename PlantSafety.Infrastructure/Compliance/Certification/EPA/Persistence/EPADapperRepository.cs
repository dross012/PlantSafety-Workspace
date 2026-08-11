// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification.EPA
//  Layer:          Infrastructure
//  File:           EPADapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Certification.EPA read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Certification.EPA.Models;
using PlantSafety.Application.Compliance.Certification.EPA.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Certification.EPA.Persistence;

/// <summary>
/// Dapper-backed repository for Certification.EPA read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class EPADapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<EPADapperRepository> logger)
    : IEPARepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EPAItemDto>> GetEPAAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEPA for Certification.EPA tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EPAItemDto>(
            "[Compliance].[usp_CertificationEPA_GetEPA]", parameters, cancellationToken);
    }
}
