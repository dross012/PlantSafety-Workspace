// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.ProcessManagement.Processes
//  Layer:          Infrastructure
//  File:           ProcessDapperRepository.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the ProcessDefinition aggregate. All DB access via
//                  stored procedures in the [ProcessManagement] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.ProcessManagement.Handlers.Processes;

namespace PlantSafety.Infrastructure.Features.ProcessManagement.Processes.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>ProcessDefinition</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class ProcessDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<ProcessDapperRepository> logger)
    : IProcessRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid processId,
        Guid tenantId,
        Guid? siteId,
        string processName,
        string category,
        Guid ownerId,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create ProcessDefinition {ProcessId} for tenant {TenantId}", processId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("ProcessId", processId)
            .AddGuid("SiteId", siteId)
            .AddString("ProcessName", processName, 200)
            .AddString("Category", category, 50)
            .AddGuid("OwnerId", ownerId)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[ProcessManagement].[usp_Process_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<ProcessReadModel?> GetByIdAsync(Guid tenantId, Guid processId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get ProcessDefinition {ProcessId} for tenant {TenantId}", processId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("ProcessId", processId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<ProcessReadModel>(
            "[ProcessManagement].[usp_Process_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<ProcessReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? category,
        string? status,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List ProcessDefinitions for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("Category", category, 50)
            .AddString("Status", status, 50)
            .AddString("OrderBy", orderBy, 100)
            .AddString("SortDirection", sortDirection, 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<ProcessListRow>(
            "[ProcessManagement].[usp_Process_List]", parameters, cancellationToken);

        var items = rows.Select(r => new ProcessReadModel
        {
            ProcessId = r.ProcessId,
            TenantId = r.TenantId,
            SiteId = r.SiteId,
            ProcessName = r.ProcessName,
            Category = r.Category,
            Status = r.Status,
            Version = r.Version,
            OwnerId = r.OwnerId,
            LastModifiedAtUtc = r.LastModifiedAtUtc,
            ETag = r.ETag,
        }).ToList();

        var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;
        return (items, totalCount);
    }

    /// <summary>Raw row shape returned by <c>[ProcessManagement].[usp_Process_List]</c>, including the windowed total row count.</summary>
    private sealed record ProcessListRow
    {
        public required Guid ProcessId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string ProcessName { get; init; }
        public required string Category { get; init; }
        public required string Status { get; init; }
        public required int Version { get; init; }
        public required Guid OwnerId { get; init; }
        public required DateTimeOffset LastModifiedAtUtc { get; init; }
        public required string ETag { get; init; }
        public required int TotalCount { get; init; }
    }
}
