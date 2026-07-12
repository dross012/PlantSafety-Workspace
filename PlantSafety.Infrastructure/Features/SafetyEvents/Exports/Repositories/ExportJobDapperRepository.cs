// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Exports
//  Layer:          Infrastructure
//  File:           ExportJobDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ExportJob aggregate. All DB access via
//                  stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.SafetyEvents.Handlers.Exports;

namespace PlantSafety.Infrastructure.Features.SafetyEvents.Exports.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>ExportJob</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ExportJobDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<ExportJobDapperRepository> logger)
    : IExportJobRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid jobId,
        Guid tenantId,
        string exportType,
        string format,
        string filterJson,
        Guid requestedByActorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create ExportJob {JobId} for tenant {TenantId}", jobId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, requestedByActorId, DateTime.UtcNow)
            .AddGuid("JobId", jobId)
            .AddString("ExportType", exportType)
            .AddString("Format", format)
            .AddString("FilterJson", filterJson, size: 8000)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_ExportJob_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<ExportJobReadModel?> GetByIdAsync(Guid tenantId, Guid jobId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get ExportJob {JobId} for tenant {TenantId}", jobId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("JobId", jobId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ExportJobReadModel>(
            "[SafetyEvents].[usp_ExportJob_GetById]", parameters, cancellationToken);
    }
}
