// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Infrastructure
//  File:           EngagementRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Engagement aggregate. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Analytics.Engagement.Persistence;

/// <summary>Dapper-backed repository for <see cref="EngagementReadModel"/> queries.</summary>
public sealed class EngagementRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<EngagementRepository> logger)
{
    /// <summary>Returns the Engagement read model for the specified identifier, or <c>null</c> if not found.</summary>
    public async Task<EngagementReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Engagement {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .AddGuid("EngagementId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<EngagementReadModel>(
            $"[Analytics].[usp_Engagement_GetById]", parameters);
    }

    /// <summary>Returns all Engagement read models for the specified tenant.</summary>
    public async Task<IReadOnlyList<EngagementReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Engagement tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<EngagementReadModel>(
            $"[Analytics].[usp_Engagement_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
