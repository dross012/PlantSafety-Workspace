// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PolicyHR
//  Layer:          Infrastructure
//  File:           HRDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PolicyHR read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.HR.Models;
using PlantSafety.Application.Compliance.Policy.HR.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policy.HR.Persistence;

/// <summary>Dapper-backed repository for <see cref="HRItemDto"/> queries.</summary>
public sealed class HRDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<HRDapperRepository> logger)
    : IHRRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<HRItemDto>> GetHRPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetHRPoliciesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<HRItemDto>(
            "[Compliance].[usp_PolicyHR_List]", parameters, cancellationToken);
    }
}