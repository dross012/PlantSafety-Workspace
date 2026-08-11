// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment.NearMissReportingRate
//  Layer:          Infrastructure
//  File:           NearMissReportingRateDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the NearMissReportingRate read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Models;
using PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Repositories;

namespace PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.NearMissReportingRate.Persistence;

/// <summary>Dapper-backed repository for NearMissReportingRate read-model queries.</summary>
public sealed class NearMissReportingRateDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<NearMissReportingRateDapperRepository> logger)
    : INearMissReportingRateRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<NearMissReportingRateItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for NearMissReportingRate tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<NearMissReportingRateItemDto>(
            "[SafetyCulture].[usp_NearMissReportingRate_GetItems]", parameters, cancellationToken);
    }
}
