// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Infrastructure
//  File:           ManagementOfChangeRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ManagementOfChange aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.ManagementOfChange.Persistence;

/// <summary>Dapper-backed repository for <see cref="ManagementOfChangeReadModel"/> queries.</summary>
public sealed class ManagementOfChangeRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ManagementOfChangeRepository> logger)
{
    /// <summary>Returns the ManagementOfChange read model or <c>null</c> if not found.</summary>
    public async Task<ManagementOfChangeReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for ManagementOfChange {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("ManagementOfChangeId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ManagementOfChangeReadModel>(
            $"[Compliance].[usp_ManagementOfChange_GetById]", parameters);
    }

    /// <summary>Returns all ManagementOfChange records for the tenant.</summary>
    public async Task<IReadOnlyList<ManagementOfChangeReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for ManagementOfChange tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<ManagementOfChangeReadModel>(
            $"[Compliance].[usp_ManagementOfChange_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
