// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.SafetyEvents.IncidentManagement
//  Layer:          Infrastructure
//  File:           IncidentReadDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper read-side repository for Incident query operations. All DB access
//                  via stored procedures under the [SafetyEvents] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;
using PlantSafety.Application.Features.SafetyEvents.Mappings.IncidentManagement;

namespace PlantSafety.Infrastructure.Features.SafetyEvents.IncidentManagement.Repositories;

/// <summary>
/// Dapper-backed read-side repository for the <c>Incident</c> aggregate.
/// Executes read-optimised stored procedures against the read-replica connection.
/// </summary>
public sealed class IncidentReadDapperRepository(
    DapperQueryExecutor queryExecutor,
    ILogger<IncidentReadDapperRepository> logger)
    : IIncidentReadRepository
{
    /// <inheritdoc/>
    public async Task<IncidentReadModel?> GetByIdAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Get Incident {IncidentId} for tenant {TenantId}", incidentId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("IncidentId", incidentId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<IncidentReadModel>(
            "[SafetyEvents].[usp_IncidentRead_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<IncidentReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? status,
        string? severity,
        DateTimeOffset? occurredFromUtc,
        DateTimeOffset? occurredToUtc,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List Incidents for tenant {TenantId} page {PageNumber}", tenantId, pageNumber);

        var listParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Status", status, size: 64)
            .AddString("Severity", severity, size: 64)
            .AddDateTime("OccurredFromUtc", occurredFromUtc?.UtcDateTime)
            .AddDateTime("OccurredToUtc", occurredToUtc?.UtcDateTime)
            .AddString("OrderBy", orderBy, size: 64)
            .AddString("SortDirection", sortDirection, size: 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var items = await queryExecutor.QueryAsync<IncidentReadModel>(
            "[SafetyEvents].[usp_IncidentRead_List]", listParameters, cancellationToken);

        var countParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Status", status, size: 64)
            .AddString("Severity", severity, size: 64)
            .AddDateTime("OccurredFromUtc", occurredFromUtc?.UtcDateTime)
            .AddDateTime("OccurredToUtc", occurredToUtc?.UtcDateTime)
            .Build();

        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int?>(
            "[SafetyEvents].[usp_IncidentRead_Count]", countParameters, cancellationToken);

        return (items, totalCount ?? 0);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<IncidentReadModel> Items, int TotalCount)> SearchAsync(
        Guid tenantId,
        string searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Search Incidents for tenant {TenantId} term {SearchTerm}", tenantId, searchTerm);

        var searchParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("SearchTerm", searchTerm, size: 200)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var items = await queryExecutor.QueryAsync<IncidentReadModel>(
            "[SafetyEvents].[usp_IncidentRead_Search]", searchParameters, cancellationToken);

        var countParameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddString("SearchTerm", searchTerm, size: 200)
            .Build();

        var totalCount = await queryExecutor.QuerySingleOrDefaultAsync<int?>(
            "[SafetyEvents].[usp_IncidentRead_SearchCount]", countParameters, cancellationToken);

        return (items, totalCount ?? 0);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<IncidentReadModel>> ExportAsync(
        Guid tenantId,
        Guid? siteId,
        DateTimeOffset? occurredFromUtc,
        DateTimeOffset? occurredToUtc,
        int maxRecords,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Export Incidents for tenant {TenantId}, max {MaxRecords}", tenantId, maxRecords);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddDateTime("OccurredFromUtc", occurredFromUtc?.UtcDateTime)
            .AddDateTime("OccurredToUtc", occurredToUtc?.UtcDateTime)
            .AddInt("MaxRecords", maxRecords)
            .Build();

        return await queryExecutor.QueryAsync<IncidentReadModel>(
            "[SafetyEvents].[usp_IncidentRead_Export]", parameters, cancellationToken);
    }
}
