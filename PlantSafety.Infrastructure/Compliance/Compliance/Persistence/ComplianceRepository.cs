// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Infrastructure
//  File:           ComplianceRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Compliance aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Compliance.Compliance.Persistence;

/// <summary>Dapper-backed repository for <see cref="ComplianceReadModel"/> queries.</summary>
public sealed class ComplianceRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ComplianceRepository> logger)
{
    /// <summary>Returns the Compliance read model or <c>null</c> if not found.</summary>
    public async Task<ComplianceReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Compliance {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("ComplianceId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ComplianceReadModel>(
            $"[Compliance].[usp_Compliance_GetById]", parameters);
    }

    /// <summary>Returns all Compliance records for the tenant.</summary>
    public async Task<IReadOnlyList<ComplianceReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Compliance tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<ComplianceReadModel>(
            $"[Compliance].[usp_Compliance_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
