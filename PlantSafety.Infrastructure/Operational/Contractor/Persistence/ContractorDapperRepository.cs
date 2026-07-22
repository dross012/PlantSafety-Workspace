// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Contractor
//  Layer:          Infrastructure
//  File:           ContractorDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Contractor read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Contractor.Models;
using PlantSafety.Application.Operational.Contractor.Repositories;

namespace PlantSafety.Infrastructure.Operational.Contractor.Persistence;

/// <summary>Dapper-backed repository for <see cref="ContractorItemDto"/> queries.</summary>
public sealed class ContractorDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ContractorDapperRepository> logger)
    : IContractorRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContractorItemDto>> GetContractorAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetContractorAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<ContractorItemDto>(
            "[Operational].[usp_Contractor_List]", parameters, cancellationToken);
    }
}
