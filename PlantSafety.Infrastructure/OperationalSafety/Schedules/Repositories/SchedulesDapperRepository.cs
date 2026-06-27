// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety.Schedules
//  Layer:          Infrastructure
//  File:           SchedulesDapperRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for Schedules aggregates. All DB access via stored procedures.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.OperationalSafety.Schedules.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>Schedules</c> module.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class SchedulesDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<SchedulesDapperRepository> logger)
    : ISchedulesInfrastructure
{
    /// <inheritdoc/>
    public async Task InsertAsync(Guid tenantId, Guid actorId, Guid correlationId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Insert Schedules for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[OperationalSafety].[usp_Schedules_Insert]", parameters, tx);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Exists check for Schedules {Id}", id);
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SchedulesId", id)
            .Build();

        var result = await queryExecutor.QuerySingleOrDefaultAsync<int?>(
            conn, "[OperationalSafety].[usp_Schedules_Exists]", parameters);

        return result.HasValue && result.Value > 0;
    }
}
