// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification.IndustrySpecific
//  Layer:          Infrastructure
//  File:           IndustrySpecificDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Certification.IndustrySpecific read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Certification.IndustrySpecific.Models;
using PlantSafety.Application.Compliance.Certification.IndustrySpecific.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Certification.IndustrySpecific.Persistence;

/// <summary>
/// Dapper-backed repository for Certification.IndustrySpecific read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class IndustrySpecificDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<IndustrySpecificDapperRepository> logger)
    : IIndustrySpecificRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<IndustrySpecificItemDto>> GetIndustrySpecificAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetIndustrySpecific for Certification.IndustrySpecific tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<IndustrySpecificItemDto>(
            "[Compliance].[usp_CertificationIndustrySpecific_GetIndustrySpecific]", parameters, cancellationToken);
    }
}
