// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyCulture.CultureAssessment.PpeComplianceRate
//  Layer:          Infrastructure
//  File:           PpeComplianceRateDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PpeComplianceRate read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyCulture.CultureAssessment.PpeComplianceRate.Models;
using PlantSafety.Application.SafetyCulture.CultureAssessment.PpeComplianceRate.Repositories;

namespace PlantSafety.Infrastructure.SafetyCulture.CultureAssessment.PpeComplianceRate.Persistence;

/// <summary>Dapper-backed repository for PpeComplianceRate read-model queries.</summary>
public sealed class PpeComplianceRateDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<PpeComplianceRateDapperRepository> logger)
    : IPpeComplianceRateRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PpeComplianceRateItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for PpeComplianceRate tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PpeComplianceRateItemDto>(
            "[SafetyCulture].[usp_PpeComplianceRate_GetItems]", parameters, cancellationToken);
    }
}
