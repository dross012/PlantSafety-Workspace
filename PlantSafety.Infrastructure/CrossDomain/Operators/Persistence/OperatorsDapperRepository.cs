// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CrossDomain.Operators
//  Layer:          Infrastructure
//  File:           OperatorsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Operators read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Operators.Models;
using PlantSafety.Application.CrossDomain.Operators.Repositories;

namespace PlantSafety.Infrastructure.CrossDomain.Operators.Persistence;

/// <summary>Dapper-backed repository for <see cref="OperatorsItemDto"/> queries.</summary>
public sealed class OperatorsDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<OperatorsDapperRepository> logger)
    : IOperatorsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<OperatorsItemDto>> GetOperatorsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetOperatorsAsync for Operators tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<OperatorsItemDto>(
            "[CrossDomain].[usp_Operators_GetOperators]", parameters, cancellationToken);
    }
}
