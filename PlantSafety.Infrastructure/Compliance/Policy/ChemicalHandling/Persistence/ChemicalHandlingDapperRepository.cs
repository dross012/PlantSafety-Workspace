// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PolicyChemicalHandling
//  Layer:          Infrastructure
//  File:           ChemicalHandlingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PolicyChemicalHandling read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.ChemicalHandling.Models;
using PlantSafety.Application.Compliance.Policy.ChemicalHandling.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policy.ChemicalHandling.Persistence;

/// <summary>Dapper-backed repository for <see cref="ChemicalHandlingItemDto"/> queries.</summary>
public sealed class ChemicalHandlingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ChemicalHandlingDapperRepository> logger)
    : IChemicalHandlingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChemicalHandlingItemDto>> GetChemicalHandlingPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetChemicalHandlingPoliciesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ChemicalHandlingItemDto>(
            "[Compliance].[usp_PolicyChemicalHandling_List]", parameters, cancellationToken);
    }
}