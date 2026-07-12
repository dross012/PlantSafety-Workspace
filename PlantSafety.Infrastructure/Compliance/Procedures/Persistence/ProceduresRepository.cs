// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Infrastructure
//  File:           ProceduresRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Procedures aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.Procedures.Persistence;

/// <summary>Dapper-backed repository for <see cref="ProceduresReadModel"/> queries.</summary>
public sealed class ProceduresRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ProceduresRepository> logger)
{
    /// <summary>Returns the Procedures read model or <c>null</c> if not found.</summary>
    public async Task<ProceduresReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Procedures {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("ProceduresId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ProceduresReadModel>(
            $"[Compliance].[usp_Procedures_GetById]", parameters);
    }

    /// <summary>Returns all Procedures records for the tenant.</summary>
    public async Task<IReadOnlyList<ProceduresReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Procedures tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<ProceduresReadModel>(
            $"[Compliance].[usp_Procedures_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
