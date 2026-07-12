// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.Operational.Training
//  Layer:          Infrastructure
//  File:           TrainingDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the WorkerTrainingRecord aggregate. All DB access via
//                  stored procedures in the [Operational] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.Operational.Handlers.Training;

namespace PlantSafety.Infrastructure.Features.Operational.Training.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>WorkerTrainingRecord</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class TrainingDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<TrainingDapperRepository> logger)
    : ITrainingRepository
{
    /// <inheritdoc/>
    public async Task AssignAsync(
        Guid recordId,
        Guid tenantId,
        Guid workerId,
        string trainingCourseCode,
        string trainingCourseName,
        string deliveryMethod,
        DateTimeOffset dueDateUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Assign Training {RecordId} for tenant {TenantId}", recordId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("RecordId", recordId)
            .AddGuid("WorkerId", workerId)
            .AddString("TrainingCourseCode", trainingCourseCode, 50)
            .AddString("TrainingCourseName", trainingCourseName, 200)
            .AddString("DeliveryMethod", deliveryMethod, 50)
            .AddDateTime("DueDateUtc", dueDateUtc.UtcDateTime)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Operational].[usp_Training_Assign]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<TrainingReadModel?> GetByIdAsync(Guid tenantId, Guid recordId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get Training {RecordId} for tenant {TenantId}", recordId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("RecordId", recordId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<TrainingReadModel>(
            "[Operational].[usp_Training_GetById]", parameters, cancellationToken);
    }
}
