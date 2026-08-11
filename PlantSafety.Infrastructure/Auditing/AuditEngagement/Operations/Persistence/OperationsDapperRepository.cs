// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement.Operations
//  Layer:          Infrastructure
//  File:           OperationsDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Operations read-model. All DB access via stored
//                  procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Auditing.AuditEngagement.Operations.Models;
using PlantSafety.Application.Auditing.AuditEngagement.Operations.Repositories;

namespace PlantSafety.Infrastructure.Auditing.AuditEngagement.Operations.Persistence;

/// <summary>Dapper-backed repository for Operations read-model queries.</summary>
public sealed class OperationsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<OperationsDapperRepository> logger)
    : IOperationsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<OperationsItemDto>> GetItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetItems for Operations tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<OperationsItemDto>(
            "[Auditing].[usp_Operations_GetItems]", parameters, cancellationToken);
    }
}
