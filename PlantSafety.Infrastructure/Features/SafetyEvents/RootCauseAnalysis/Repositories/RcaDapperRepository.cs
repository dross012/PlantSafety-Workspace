// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Infrastructure
//  File:           RcaDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the RootCauseAnalysis aggregate. All DB access via
//                  stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.SafetyEvents.Handlers.RootCauseAnalysis;

namespace PlantSafety.Infrastructure.Features.SafetyEvents.RootCauseAnalysis.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>RootCauseAnalysis</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class RcaDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<RcaDapperRepository> logger)
    : IRcaRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid rcaId,
        Guid tenantId,
        Guid linkedEventId,
        string linkedEventType,
        string methodology,
        string rootCauses,
        string correctiveActions,
        Guid leadInvestigatorId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create Rca {RcaId} for tenant {TenantId}", rcaId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("RcaId", rcaId)
            .AddGuid("LinkedEventId", linkedEventId)
            .AddString("LinkedEventType", linkedEventType)
            .AddString("Methodology", methodology)
            .AddString("RootCauses", rootCauses, size: 8000)
            .AddString("CorrectiveActions", correctiveActions, size: 8000)
            .AddGuid("LeadInvestigatorId", leadInvestigatorId)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_Rca_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<RcaReadModel?> GetByIdAsync(Guid tenantId, Guid rcaId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get Rca {RcaId} for tenant {TenantId}", rcaId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("RcaId", rcaId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<RcaReadModel>(
            "[SafetyEvents].[usp_Rca_GetById]", parameters, cancellationToken);
    }
}
