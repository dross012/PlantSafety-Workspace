// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PolicyPPE
//  Layer:          Infrastructure
//  File:           PPEDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PolicyPPE read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.PPE.Models;
using PlantSafety.Application.Compliance.Policy.PPE.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policy.PPE.Persistence;

/// <summary>Dapper-backed repository for <see cref="PPEItemDto"/> queries.</summary>
public sealed class PPEDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PPEDapperRepository> logger)
    : IPPERepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PPEItemDto>> GetPPEPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetPPEPoliciesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<PPEItemDto>(
            "[Compliance].[usp_PolicyPPE_List]", parameters, cancellationToken);
    }
}