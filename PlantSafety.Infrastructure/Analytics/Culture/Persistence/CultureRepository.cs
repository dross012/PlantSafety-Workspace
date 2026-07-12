// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Infrastructure
//  File:           CultureRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Culture aggregate. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Analytics.Culture.Persistence;

/// <summary>Dapper-backed repository for <see cref="CultureReadModel"/> queries.</summary>
public sealed class CultureRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<CultureRepository> logger)
{
    /// <summary>Returns the Culture read model for the specified identifier, or <c>null</c> if not found.</summary>
    public async Task<CultureReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Culture {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .AddGuid("CultureId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CultureReadModel>(
            $"[Analytics].[usp_Culture_GetById]", parameters);
    }

    /// <summary>Returns all Culture read models for the specified tenant.</summary>
    public async Task<IReadOnlyList<CultureReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Culture tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid(SqlConstants.ParamTenantId.TrimStart('@'), tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<CultureReadModel>(
            $"[Analytics].[usp_Culture_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
