// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PolicyEnvironmental
//  Layer:          Infrastructure
//  File:           EnvironmentalDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PolicyEnvironmental read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.Environmental.Models;
using PlantSafety.Application.Compliance.Policy.Environmental.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policy.Environmental.Persistence;

/// <summary>Dapper-backed repository for <see cref="EnvironmentalItemDto"/> queries.</summary>
public sealed class EnvironmentalDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EnvironmentalDapperRepository> logger)
    : IEnvironmentalRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EnvironmentalItemDto>> GetEnvironmentalPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEnvironmentalPoliciesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EnvironmentalItemDto>(
            "[Compliance].[usp_PolicyEnvironmental_List]", parameters, cancellationToken);
    }
}