// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PolicySafetyPolicies
//  Layer:          Infrastructure
//  File:           SafetyPoliciesDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PolicySafetyPolicies read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.SafetyPolicies.Models;
using PlantSafety.Application.Compliance.Policy.SafetyPolicies.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policy.SafetyPolicies.Persistence;

/// <summary>Dapper-backed repository for <see cref="SafetyPoliciesItemDto"/> queries.</summary>
public sealed class SafetyPoliciesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<SafetyPoliciesDapperRepository> logger)
    : ISafetyPoliciesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<SafetyPoliciesItemDto>> GetSafetyPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetSafetyPoliciesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<SafetyPoliciesItemDto>(
            "[Compliance].[usp_PolicySafetyPolicies_List]", parameters, cancellationToken);
    }
}