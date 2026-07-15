// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PolicyContractor
//  Layer:          Infrastructure
//  File:           ContractorDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PolicyContractor read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.Contractor.Models;
using PlantSafety.Application.Compliance.Policy.Contractor.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policy.Contractor.Persistence;

/// <summary>Dapper-backed repository for <see cref="ContractorItemDto"/> queries.</summary>
public sealed class ContractorDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ContractorDapperRepository> logger)
    : IContractorRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContractorItemDto>> GetContractorPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetContractorPoliciesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ContractorItemDto>(
            "[Compliance].[usp_PolicyContractor_List]", parameters, cancellationToken);
    }
}