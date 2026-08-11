// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement.RegulatoryCompliance
//  Layer:          Infrastructure
//  File:           RegulatoryComplianceDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RegulatoryCompliance read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditEngagement.RegulatoryCompliance.Models;
using PlantSafety.Application.Auditing.AuditEngagement.RegulatoryCompliance.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditEngagement.RegulatoryCompliance.Persistence;

/// <summary>Dapper-backed repository for RegulatoryCompliance read-model queries.</summary>
public sealed class RegulatoryComplianceDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<RegulatoryComplianceDapperRepository> logger)
    : IRegulatoryComplianceRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<RegulatoryComplianceItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for RegulatoryCompliance tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RegulatoryComplianceItemDto>(
            "[Auditing].[usp_RegulatoryCompliance_GetItems]", parameters, cancellationToken);
    }
}
