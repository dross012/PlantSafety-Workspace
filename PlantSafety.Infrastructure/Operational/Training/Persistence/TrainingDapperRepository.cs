// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Training
//  Layer:          Infrastructure
//  File:           TrainingDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Training read-model. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Operational.Training.Models;
using PlantSafety.Application.Operational.Training.Repositories;

namespace PlantSafety.Infrastructure.Operational.Training.Persistence;

/// <summary>Dapper-backed repository for <see cref="TrainingItemDto"/> queries.</summary>
public sealed class TrainingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    ILogger<TrainingDapperRepository> logger)
    : ITrainingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<TrainingItemDto>> GetTrainingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTrainingAsync for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TrainingItemDto>(
            "[Operational].[usp_Training_List]", parameters, cancellationToken);
    }
}
