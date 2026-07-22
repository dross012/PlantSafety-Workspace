// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement.QualityControl
//  Layer:          Infrastructure
//  File:           QualityControlDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the QualityControl read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditEngagement.QualityControl.Models;
using PlantSafety.Application.Auditing.AuditEngagement.QualityControl.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditEngagement.QualityControl.Persistence;

/// <summary>Dapper-backed repository for QualityControl read-model queries.</summary>
public sealed class QualityControlDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<QualityControlDapperRepository> logger)
    : IQualityControlRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<QualityControlItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for QualityControl tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<QualityControlItemDto>(
            "[Auditing].[usp_QualityControl_GetItems]", parameters, cancellationToken);
    }
}
