// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AITraining
//  Layer:          Infrastructure
//  File:           AITrainingDapperRepository.cs
//  Created:        2026-07-22
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the AITraining read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Intelligence.AITraining.Models;
using PlantSafety.Application.Intelligence.AITraining.Repositories;

namespace PlantSafety.Infrastructure.Intelligence.AITraining.Persistence;

/// <summary>
/// Dapper-backed repository for AITraining read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class AITrainingDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<AITrainingDapperRepository> logger)
    : IAITrainingRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<AITrainingItemDto>> GetTrainingJobsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("GetTrainingJobsAsync for AITraining tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<AITrainingItemDto>(
            "[Intelligence].[usp_AITraining_GetTrainingJobs]", parameters, cancellationToken);
    }
}
