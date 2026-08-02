// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.DrillsAndExercises
//  Layer:          Infrastructure
//  File:           DrillsAndExercisesDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the DrillsAndExercises read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Models;
using PlantSafety.Application.SafetyEvents.DrillsAndExercises.Repositories;

namespace PlantSafety.Infrastructure.SafetyEvents.DrillsAndExercises.Persistence;

/// <summary>
/// Dapper-backed repository for DrillsAndExercises read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class DrillsAndExercisesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<DrillsAndExercisesDapperRepository> logger)
    : IDrillsAndExercisesRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<DrillsAndExercisesItemDto>> GetDrillsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetDrills for DrillsAndExercises tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<DrillsAndExercisesItemDto>(
            "[SafetyEvents].[usp_DrillsAndExercises_GetDrills]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DrillsAndExercisesDetailDto?> GetByIdAsync(Guid drillsAndExercisesId, Guid tenantId, CancellationToken cancellationToken)
    {
        logger.LogDebug("GetById for DrillsAndExercises {DrillsAndExercisesId} Tenant {TenantId}", drillsAndExercisesId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("DrillsAndExercisesId", drillsAndExercisesId)
            .AddGuid("TenantId", tenantId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<DrillsAndExercisesDetailDto>(
            "[SafetyEvents].[usp_DrillsAndExercises_GetById]", parameters, cancellationToken);
    }
}
