// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Infrastructure
//  File:           PermitsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Permits aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.Permits.Persistence;

/// <summary>Dapper-backed repository for <see cref="PermitsReadModel"/> queries.</summary>
public sealed class PermitsRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PermitsRepository> logger)
{
    /// <summary>Returns the Permits read model or <c>null</c> if not found.</summary>
    public async Task<PermitsReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Permits {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("PermitsId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PermitsReadModel>(
            $"[Compliance].[usp_Permits_GetById]", parameters);
    }

    /// <summary>Returns all Permits records for the tenant.</summary>
    public async Task<IReadOnlyList<PermitsReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Permits tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<PermitsReadModel>(
            $"[Compliance].[usp_Permits_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
