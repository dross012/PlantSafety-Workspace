// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Infrastructure
//  File:           ObservationRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for Observation aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.SafetyEvents.Observation.Persistence;

/// <summary>Dapper-backed repository for <see cref="ObservationReadModel"/> queries.</summary>
public sealed class ObservationRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<ObservationRepository> logger)
{
    /// <summary>Returns the Observation read model or <c>null</c> if not found.</summary>
    public async Task<ObservationReadModel?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Observation {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("ObservationId", id)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ObservationReadModel>(
            $"[SafetyEvents].[usp_Observation_GetById]", parameters);
    }

    /// <summary>Returns all Observation records for the tenant.</summary>
    public async Task<IReadOnlyList<ObservationReadModel>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetAll for Observation tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .Build();

        var result = await queryExecutor.QueryAsync<ObservationReadModel>(
            $"[SafetyEvents].[usp_Observation_GetAll]", parameters);

        return result.ToList().AsReadOnly();
    }
}
