// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits.PressureTesting
//  Layer:          Infrastructure
//  File:           PressureTestingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits.PressureTesting read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Permits.PressureTesting.Models;
using PlantSafety.Application.Compliance.Permits.PressureTesting.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Permits.PressureTesting.Persistence;

/// <summary>
/// Dapper-backed repository for Permits.PressureTesting read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class PressureTestingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PressureTestingDapperRepository> logger)
    : IPressureTestingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PressureTestingItemDto>> GetPressureTestingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPressureTesting for Permits.PressureTesting tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PressureTestingItemDto>(
            "[Compliance].[usp_PermitPressureTesting_GetPressureTesting]", parameters, cancellationToken);
    }
}
