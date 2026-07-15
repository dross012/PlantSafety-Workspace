// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification.Training
//  Layer:          Infrastructure
//  File:           TrainingDapperRepository.cs
//  Created:        2026-07-13
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the Certification.Training read-model. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Compliance.Certification.Training.Models;
using PlantSafety.Application.Compliance.Certification.Training.Repositories;

namespace PlantSafety.Infrastructure.Compliance.Certification.Training.Persistence;

/// <summary>
/// Dapper-backed repository for Certification.Training read-model queries.
/// Executes stored procedures for all persistence operations.
/// </summary>
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
        logger.LogDebug("GetTraining for Certification.Training tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("FromUtc", from)
            .AddDateTime("ToUtc", to)
            .Build();

        return await queryExecutor.QueryAsync<TrainingItemDto>(
            "[Compliance].[usp_CertificationTraining_GetTraining]", parameters, cancellationToken);
    }
}
