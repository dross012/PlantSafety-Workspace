// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification.DOT
//  Layer:          Infrastructure
//  File:           DOTDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Certification.DOT read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Certification.DOT.Models;
using PlantSafety.Application.Compliance.Certification.DOT.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Certification.DOT.Persistence;

/// <summary>
/// Dapper-backed repository for Certification.DOT read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class DOTDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<DOTDapperRepository> logger)
    : IDOTRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<DOTItemDto>> GetDOTAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDOT for Certification.DOT tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<DOTItemDto>(
            "[Compliance].[usp_CertificationDOT_GetDOT]", parameters, cancellationToken);
    }
}
