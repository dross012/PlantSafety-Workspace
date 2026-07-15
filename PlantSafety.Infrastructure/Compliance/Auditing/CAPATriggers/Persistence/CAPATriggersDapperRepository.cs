// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing.CAPATriggers
//  Layer:          Infrastructure
//  File:           CAPATriggersDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CAPATriggers read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Auditing.CAPATriggers.Models;
using PlantSafety.Application.Compliance.Auditing.CAPATriggers.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Auditing.CAPATriggers.Persistence;

/// <summary>
/// Dapper-backed repository for CAPATriggers read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class CAPATriggersDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<CAPATriggersDapperRepository> logger)
    : ICAPATriggersRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<CAPATriggerItemDto>> GetTriggersAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTriggers for CAPATriggers tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<CAPATriggerItemDto>(
            "[Compliance].[usp_CAPATriggers_GetTriggers]", parameters, cancellationToken);
    }
}
