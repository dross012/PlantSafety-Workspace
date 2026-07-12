// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.ContinuousImprovement.CAPA
//  Layer:          Infrastructure
//  File:           CorrectiveActionDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the CorrectiveAction aggregate. All DB access via
//                  stored procedures in the [ContinuousImprovement] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;

namespace PlantSafety.Infrastructure.Features.ContinuousImprovement.CAPA.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>CorrectiveAction</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class CorrectiveActionDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    ILogger<CorrectiveActionDapperRepository> logger)
    : ICorrectiveActionRepository
{
    /// <inheritdoc/>
    public async Task CreateAsync(
        Guid caId,
        Guid tenantId,
        Guid sourceEventId,
        string sourceEventType,
        string title,
        string description,
        string actionType,
        string priority,
        Guid assignedToWorkerId,
        DateTimeOffset dueAtUtc,
        Guid actorId,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Create CorrectiveAction {CaId} for tenant {TenantId}", caId, tenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(tenantId, correlationId, actorId, DateTime.UtcNow)
            .AddGuid("CaId", caId)
            .AddGuid("SourceEventId", sourceEventId)
            .AddString("SourceEventType", sourceEventType, 100)
            .AddString("Title", title, 200)
            .AddString("Description", description, 4000)
            .AddString("ActionType", actionType, 50)
            .AddString("Priority", priority, 50)
            .AddGuid("AssignedToWorkerId", assignedToWorkerId)
            .AddDateTime("DueAtUtc", dueAtUtc.UtcDateTime)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[ContinuousImprovement].[usp_CorrectiveAction_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <inheritdoc/>
    public async Task<CorrectiveActionReadModel?> GetByIdAsync(Guid tenantId, Guid caId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Get CorrectiveAction {CaId} for tenant {TenantId}", caId, tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("CaId", caId)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<CorrectiveActionReadModel>(
            "[ContinuousImprovement].[usp_CorrectiveAction_GetById]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<CorrectiveActionReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? sourceEventId,
        string? actionType,
        string? priority,
        string? status,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("List CorrectiveActions for tenant {TenantId}", tenantId);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SourceEventId", sourceEventId)
            .AddString("ActionType", actionType, 50)
            .AddString("Priority", priority, 50)
            .AddString("Status", status, 50)
            .AddString("OrderBy", orderBy, 100)
            .AddString("SortDirection", sortDirection, 4)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<CorrectiveActionListRow>(
            "[ContinuousImprovement].[usp_CorrectiveAction_List]", parameters, cancellationToken);

        var items = rows.Select(r => new CorrectiveActionReadModel
        {
            CaId = r.CaId,
            TenantId = r.TenantId,
            SourceEventId = r.SourceEventId,
            SourceEventType = r.SourceEventType,
            Title = r.Title,
            Description = r.Description,
            ActionType = r.ActionType,
            Priority = r.Priority,
            AssignedToWorkerId = r.AssignedToWorkerId,
            DueAtUtc = r.DueAtUtc,
            Status = r.Status,
            CompletedAtUtc = r.CompletedAtUtc,
            LastModifiedAtUtc = r.LastModifiedAtUtc,
            ETag = r.ETag,
        }).ToList();

        var totalCount = rows.Count > 0 ? rows[0].TotalCount : 0;
        return (items, totalCount);
    }

    /// <summary>Raw row shape returned by <c>[ContinuousImprovement].[usp_CorrectiveAction_List]</c>, including the windowed total row count.</summary>
    private sealed record CorrectiveActionListRow
    {
        public required Guid CaId { get; init; }
        public required Guid TenantId { get; init; }
        public required Guid SourceEventId { get; init; }
        public required string SourceEventType { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required string ActionType { get; init; }
        public required string Priority { get; init; }
        public required Guid AssignedToWorkerId { get; init; }
        public required DateTimeOffset DueAtUtc { get; init; }
        public required string Status { get; init; }
        public DateTimeOffset? CompletedAtUtc { get; init; }
        public required DateTimeOffset LastModifiedAtUtc { get; init; }
        public required string ETag { get; init; }
        public required int TotalCount { get; init; }
    }
}
