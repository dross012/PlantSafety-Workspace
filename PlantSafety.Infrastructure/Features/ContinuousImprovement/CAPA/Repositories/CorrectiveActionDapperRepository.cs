// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Features.ContinuousImprovement.CAPA
//  Layer:          Infrastructure
//  File:           CorrectiveActionDapperRepository.cs
//  Created:        2026-07-12
//  Author:         PlantSafety Platform Team
//  Updated:        2026-08-06 -- CreateAsync(primitives) replaced by AddAsync(CorrectiveAction
//                  aggregate): the aggregate (Domain.ContinuousImprovement.CAPA.Aggregates.
//                  CorrectiveAction) previously existed but was never actually constructed by any
//                  command handler, so CorrectiveActionCreatedEvent was never raised despite
//                  CrossModuleDomainEventMapper having a registered case arm for it. AddAsync now
//                  persists the real aggregate and relays its domain events to both the outbox and
//                  (for the curated subset with a registered cross-module DTO) Shared.EventBus,
//                  mirroring RiskDapperRepository/AccidentDapperRepository/
//                  ComplianceEvaluationDapperRepository's Publish-After-Commit pattern.
//  SchemaVersion:  2026.2
//  Description:    Dapper repository for the CorrectiveAction aggregate. All DB access via
//                  stored procedures in the [ContinuousImprovement] schema. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Aggregates;
using PlantSafety.Infrastructure.Messaging.CrossModule;

namespace PlantSafety.Infrastructure.Features.ContinuousImprovement.CAPA.Repositories;

/// <summary>
/// Dapper-backed repository for the <c>CorrectiveAction</c> aggregate.
/// Executes stored procedures for all persistence operations.
/// </summary>
public sealed class CorrectiveActionDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<CorrectiveActionDapperRepository> logger,
    ICrossModuleEventRelay? crossModuleEventRelay = null)
    : ICorrectiveActionRepository
{
    /// <inheritdoc/>
    public async Task AddAsync(CorrectiveAction aggregate, Guid actorId, Guid correlationId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Add CorrectiveAction {CaId} for tenant {TenantId}", aggregate.CorrectiveActionId.Value, aggregate.TenantId);
        using var conn = connectionFactory.CreateConnection();
        using var tx   = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, correlationId, actorId, aggregate.CreatedAtUtc.Value)
            .AddGuid("CaId", aggregate.CorrectiveActionId.Value)
            .AddGuid("SourceEventId", aggregate.SourceEventId)
            .AddString("SourceEventType", aggregate.SourceEventType, 100)
            .AddString("Title", aggregate.Title, 200)
            .AddString("Description", aggregate.Description, 4000)
            .AddString("ActionType", aggregate.ActionType, 50)
            .AddString("Priority", aggregate.Priority, 50)
            .AddGuid("AssignedToWorkerId", aggregate.AssignedToWorkerId.Value)
            .AddDateTime("DueAtUtc", aggregate.DueAtUtc.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[ContinuousImprovement].[usp_CorrectiveAction_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), actorId.ToString(), correlationId.ToString());
            crossModuleEventRelay?.Enqueue(domainEvent, aggregate.TenantId.ToString(), actorId.ToString(), correlationId.ToString());
        }
        aggregate.ClearDomainEvents();

        // Publish-After-Commit: the SQL transaction above has already committed, so it is safe to
        // relay onto Shared.EventBus now (mirrors UnitOfWork.CommitAsync's discipline).
        if (crossModuleEventRelay is { HasPendingEvents: true })
            await crossModuleEventRelay.FlushAsync(cancellationToken);
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
