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

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<EmergencyResponseReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? status,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List EmergencyResponses for tenant {TenantId} page {PageNumber}", tenantId, pageNumber);

        var listParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Status", status, size: 64)
            .AddString("OrderBy", orderBy, size: 64)
            .AddString("SortDirection", sortDirection, size: 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var items = await queryExecutor.QueryAsync<EmergencyResponseReadModel>(
            "[SafetyEvents].[usp_EmergencyResponse_List]", listParameters, cancellationToken);

        var countParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Status", status, size: 64)
            .Build();

        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int?>(
            "[SafetyEvents].[usp_EmergencyResponse_Count]", countParameters, cancellationToken);

        return (items, totalCount ?? 0);
    }
}
