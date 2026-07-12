// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Training.CompetencyManagement
//  Layer:          Infrastructure
//  File:           CompetencyManagementDapperRepository.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for CompetencyManagement aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Infrastructure.Training.CompetencyManagement.Repositories;

/// <summary>Dapper repository for the <c>CompetencyManagement</c> module.</summary>
public sealed class CompetencyManagementDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<CompetencyManagementDapperRepository> logger)
    : ICompetencyManagementInfrastructure
{
    /// <inheritdoc/>
    public async Task InsertAsync(Guid tenantId, Guid actorId, Guid correlationId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Insert CompetencyManagement for tenant {TenantId}", tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Training].[usp_CompetencyManagement_Insert]", parameters, tx);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("CompetencyManagementId", id)
            .Build();

        var result = await queryExecutor.QuerySingleOrDefaultAsync<int?>(
            "[Training].[usp_CompetencyManagement_Exists]", parameters);

        return result.HasValue && result.Value > 0;
    }
}
