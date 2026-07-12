// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Infrastructure
//  File:           PredictiveAnalyticsRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the PredictiveAnalytics aggregate. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Analytics.PredictiveAnalytics.Persistence;

/// <summary>Dapper-backed repository for <see cref="PredictiveAnalyticsReadModel"/> queries.</summary>
public sealed class PredictiveAnalyticsRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<PredictiveAnalyticsRepository> logger)
{
    /// <summary>Returns the PredictiveAnalytics read model for the specified identifier, or <c>null</c> if not found.</summary>
    public async Task<PredictiveAnalyticsReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PredictiveAnalytics {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .AddGuid("PredictiveAnalyticsId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<PredictiveAnalyticsReadModel>(
            $"[Analytics].[usp_PredictiveAnalytics_GetById]", parameters);
    }

    /// <summary>Returns all PredictiveAnalytics read models for the specified tenant.</summary>
    public async Task<IReadOnlyList<PredictiveAnalyticsReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for PredictiveAnalytics tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<PredictiveAnalyticsReadModel>(
            $"[Analytics].[usp_PredictiveAnalytics_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
