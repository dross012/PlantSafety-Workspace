// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Infrastructure
//  File:           AuditingRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Auditing aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.Auditing.Persistence;

/// <summary>Dapper-backed repository for <see cref="AuditingReadModel"/> queries.</summary>
public sealed class AuditingRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<AuditingRepository> logger)
{
    /// <summary>Returns the Auditing read model or <c>null</c> if not found.</summary>
    public async Task<AuditingReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Auditing {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("AuditingId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<AuditingReadModel>(
            conn, $"[Compliance].[usp_Auditing_GetById]", parameters);
    }

    /// <summary>Returns all Auditing records for the tenant.</summary>
    public async Task<IReadOnlyList<AuditingReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Auditing tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<AuditingReadModel>(
            conn, $"[Compliance].[usp_Auditing_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
