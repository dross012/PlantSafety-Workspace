// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Infrastructure
//  File:           EmergencyResponseDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the EmergencyResponse aggregate. All DB access via
//                  stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.SafetyEvents.Handlers.EmergencyManagement;

namespace PlantSafety.Infrastructure.Features.SafetyEvents.EmergencyManagement.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>EmergencyResponse</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class EmergencyResponseDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<EmergencyResponseDapperRepository> logger)
    : IEmergencyResponseRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid responseId,
        Guid tenantId,
        Guid siteId,
        Guid? linkedCrisisEventId,
        string responsePlanName,
        string description,
        DateTimeOffset activatedAtUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create EmergencyResponse {ResponseId} for tenant {TenantId}", responseId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("ResponseId", responseId)
            .AddGuid("SiteId", siteId)
            .AddGuid("LinkedCrisisEventId", linkedCrisisEventId)
            .AddString("ResponsePlanName", responsePlanName)
            .AddString("Description", description)
            .AddDateTime("ActivatedAtUtc", activatedAtUtc.UtcDateTime)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_EmergencyResponse_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<EmergencyResponseReadModel?> GetByIdAsync(Guid tenantId, Guid responseId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get EmergencyResponse {ResponseId} for tenant {TenantId}", responseId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("ResponseId", responseId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<EmergencyResponseReadModel>(
            "[SafetyEvents].[usp_EmergencyResponse_GetById]", parameters, cancellationToken);
    }
}
