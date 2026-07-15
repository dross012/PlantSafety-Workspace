// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ProceduresHazMatProcedures
//  Layer:          Infrastructure
//  File:           HazMatProceduresDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ProceduresHazMatProcedures read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Procedures.HazMatProcedures.Models;
using PlantSafety.Application.Compliance.Procedures.HazMatProcedures.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Procedures.HazMatProcedures.Persistence;

/// <summary>Dapper-backed repository for <see cref="HazMatProceduresItemDto"/> queries.</summary>
public sealed class HazMatProceduresDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<HazMatProceduresDapperRepository> logger)
    : IHazMatProceduresRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<HazMatProceduresItemDto>> GetHazMatProceduresAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHazMatProceduresAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HazMatProceduresItemDto>(
            "[Compliance].[usp_ProceduresHazMatProcedures_List]", parameters, cancellationToken);
    }
}