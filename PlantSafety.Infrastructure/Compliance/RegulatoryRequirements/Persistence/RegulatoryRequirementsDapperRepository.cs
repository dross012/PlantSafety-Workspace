// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryRequirements
//  Layer:          Infrastructure
//  File:           RegulatoryRequirementsDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RegulatoryRequirements read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.RegulatoryRequirements.Models;
using PlantSafety.Application.Compliance.RegulatoryRequirements.Repositories;

namespace PlantSafety.Infrastructure.Compliance.RegulatoryRequirements.Persistence;

/// <summary>Dapper-backed repository for <see cref="RegulatoryRequirementsItemDto"/> queries.</summary>
public sealed class RegulatoryRequirementsDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<RegulatoryRequirementsDapperRepository> logger)
    : IRegulatoryRequirementsRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<RegulatoryRequirementsItemDto>> GetRegulatoryRequirementsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetRegulatoryRequirementsAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<RegulatoryRequirementsItemDto>(
            "[Compliance].[usp_RegulatoryRequirements_List]", parameters, cancellationToken);
    }
}