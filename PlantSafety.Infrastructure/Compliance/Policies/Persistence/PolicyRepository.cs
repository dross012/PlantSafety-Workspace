// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Infrastructure
//  File:           PolicyRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Policy aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.Policies.Persistence;

/// <summary>Dapper-backed repository for <see cref="PolicyReadModel"/> queries.</summary>
public sealed class PolicyRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PolicyRepository> logger)
{
    /// <summary>Returns the Policy read model or <c>null</c> if not found.</summary>
    public async Task<PolicyReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Policy {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("PolicyId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PolicyReadModel>(
            conn, $"[Compliance].[usp_Policy_GetById]", parameters);
    }

    /// <summary>Returns all Policy records for the tenant.</summary>
    public async Task<IReadOnlyList<PolicyReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Policy tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<PolicyReadModel>(
            conn, $"[Compliance].[usp_Policy_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
