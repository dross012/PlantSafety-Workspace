// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PolicyEmergency
//  Layer:          Infrastructure
//  File:           EmergencyDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PolicyEmergency read-model aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Policy.Emergency.Models;
using PlantSafety.Application.Compliance.Policy.Emergency.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Policy.Emergency.Persistence;

/// <summary>Dapper-backed repository for <see cref="EmergencyItemDto"/> queries.</summary>
public sealed class EmergencyDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EmergencyDapperRepository> logger)
    : IEmergencyRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<EmergencyItemDto>> GetEmergencyPoliciesAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetEmergencyPoliciesAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<EmergencyItemDto>(
            "[Compliance].[usp_PolicyEmergency_List]", parameters, cancellationToken);
    }
}