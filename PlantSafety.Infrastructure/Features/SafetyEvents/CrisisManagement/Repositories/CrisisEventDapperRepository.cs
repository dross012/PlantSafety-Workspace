// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Infrastructure
//  File:           CrisisEventDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CrisisEvent aggregate. All DB access via
//                  stored procedures. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.SafetyEvents.Handlers.CrisisManagement;

namespace PlantSafety.Infrastructure.Features.SafetyEvents.CrisisManagement.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>CrisisEvent</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class CrisisEventDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<CrisisEventDapperRepository> logger)
    : ICrisisEventRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid crisisEventId,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        string crisisCategory,
        Guid incidentCommanderId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create CrisisEvent {CrisisEventId} for tenant {TenantId}", crisisEventId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("CrisisEventId", crisisEventId)
            .AddGuid("SiteId", siteId)
            .AddString("Title", title)
            .AddString("Description", description)
            .AddString("CrisisCategory", crisisCategory)
            .AddGuid("IncidentCommanderId", incidentCommanderId)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyEvents].[usp_CrisisEvent_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<CrisisEventReadModel?> GetByIdAsync(Guid tenantId, Guid crisisEventId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get CrisisEvent {CrisisEventId} for tenant {TenantId}", crisisEventId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("CrisisEventId", crisisEventId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CrisisEventReadModel>(
            "[SafetyEvents].[usp_CrisisEvent_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<CrisisEventReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? status,
        string? crisisCategory,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List CrisisEvents for tenant {TenantId} page {PageNumber}", tenantId, pageNumber);

        var listParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Status", status, size: 64)
            .AddString("CrisisCategory", crisisCategory, size: 64)
            .AddString("OrderBy", orderBy, size: 64)
            .AddString("SortDirection", sortDirection, size: 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var items = await queryExecutor.QueryAsync<CrisisEventReadModel>(
            "[SafetyEvents].[usp_CrisisEvent_List]", listParameters, cancellationToken);

        var countParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Status", status, size: 64)
            .AddString("CrisisCategory", crisisCategory, size: 64)
            .Build();

        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int?>(
            "[SafetyEvents].[usp_CrisisEvent_Count]", countParameters, cancellationToken);

        return (items, totalCount ?? 0);
    }
}
