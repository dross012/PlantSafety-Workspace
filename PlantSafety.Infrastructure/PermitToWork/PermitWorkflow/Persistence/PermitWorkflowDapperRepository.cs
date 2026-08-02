// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Infrastructure
//  File:           PermitWorkflowDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IPermitWorkflowRepository (Domain repository
//                  interface for the PermitWorkflow aggregate root). All DB access via stored
//                  procedures in the [PermitToWork] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because PermitWorkflow exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
//  UPDATED:        2026-07-29 -- UpdateAsync now takes an @ETag optimistic-concurrency token
//                  (Gap 2 remediation) and invokes usp_PermitWorkflow_Update via
//                  ExecuteScalarAsync (not ExecuteAsync) so the proc's rows-affected SELECT can
//                  be read back; zero rows affected throws ConcurrencyConflictException. Mirrors
//                  IncidentDapperRepository.UpdateAsync. PermitWorkflowRow gains an ETag field,
//                  populated by usp_PermitWorkflow_GetById's RowVersion projection.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates;
using PlantSafety.Domain.PermitToWork.PermitWorkflow.Enums;
using PlantSafety.Domain.PermitToWork.PermitWorkflow.Events;
using PlantSafety.Domain.PermitToWork.PermitWorkflow.Interfaces;
using PlantSafety.Domain.PermitToWork.PermitWorkflow.Specifications;
using PlantSafety.Domain.PermitToWork.PermitWorkflow.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.PermitToWork.PermitWorkflow.Persistence;

/// <summary>Dapper-backed repository for the <see cref="PermitWorkflow"/> aggregate root.</summary>
public sealed class PermitWorkflowDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<PermitWorkflowDapperRepository> logger)
    : IPermitWorkflowRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow?> GetByIdAsync(PermitWorkflowId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for PermitWorkflow {PermitWorkflowId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("PermitWorkflowId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<PermitWorkflowRow>(
            "[PermitToWork].[usp_PermitWorkflow_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow>> ListAsync(PermitWorkflowSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List PermitWorkflow via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<PermitWorkflowRow>(
            "[PermitToWork].[usp_PermitWorkflow_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add PermitWorkflow {PermitWorkflowId}", aggregate.PermitWorkflowId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitWorkflowId", aggregate.PermitWorkflowId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PermitNumber", aggregate.PermitNumber, 400)
            .AddString("PermitTypeCode", aggregate.PermitTypeCode, 400)
            .AddGuid("RequesterUserId", aggregate.RequesterUserId?.Value)
            .AddGuid("IssuerUserId", aggregate.IssuerUserId?.Value)
            .AddString("WorkDescription", aggregate.WorkDescription, 400)
            .AddString("RequiredControls", aggregate.RequiredControls, 400)
            .AddInt("RiskScore", aggregate.RiskScore)
            .AddString("ControlsConfirmedBy", aggregate.ControlsConfirmedBy, 400)
            .AddDateTime("ControlsImplementedAtUtc", aggregate.ControlsImplementedAtUtc?.Value)
            .AddGuid("ApprovedBy", aggregate.ApprovedBy?.Value)
            .AddDateTime("ApprovedAtUtc", aggregate.ApprovedAtUtc?.Value)
            .AddDateTime("IssuedAtUtc", aggregate.IssuedAtUtc?.Value)
            .AddDateTime("ExpiresAtUtc", aggregate.ExpiresAtUtc?.Value)
            .AddGuid("ClosureSignedBy", aggregate.ClosureSignedBy?.Value)
            .AddString("ClosureNotes", aggregate.ClosureNotes, 400)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddString("RejectedReason", aggregate.RejectedReason, 400)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[PermitToWork].[usp_PermitWorkflow_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    
        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events
        // raised by the aggregate during this write so TransactionalUnitOfWork.CommitAsync
        // (invoked by TransactionBehavior after the handler returns) can flush them to
        // [System].[OutboxMessages]. See TransactionalUnitOfWork remarks for the known
        // eventual-consistency gap (outbox flush is not in the same SQL transaction).
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
}

    /// <inheritdoc/>
    public async Task UpdateAsync(PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update PermitWorkflow {PermitWorkflowId}", aggregate.PermitWorkflowId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("PermitWorkflowId", aggregate.PermitWorkflowId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("PermitNumber", aggregate.PermitNumber, 400)
            .AddString("PermitTypeCode", aggregate.PermitTypeCode, 400)
            .AddGuid("RequesterUserId", aggregate.RequesterUserId?.Value)
            .AddGuid("IssuerUserId", aggregate.IssuerUserId?.Value)
            .AddString("WorkDescription", aggregate.WorkDescription, 400)
            .AddString("RequiredControls", aggregate.RequiredControls, 400)
            .AddInt("RiskScore", aggregate.RiskScore)
            .AddString("ControlsConfirmedBy", aggregate.ControlsConfirmedBy, 400)
            .AddDateTime("ControlsImplementedAtUtc", aggregate.ControlsImplementedAtUtc?.Value)
            .AddGuid("ApprovedBy", aggregate.ApprovedBy?.Value)
            .AddDateTime("ApprovedAtUtc", aggregate.ApprovedAtUtc?.Value)
            .AddDateTime("IssuedAtUtc", aggregate.IssuedAtUtc?.Value)
            .AddDateTime("ExpiresAtUtc", aggregate.ExpiresAtUtc?.Value)
            .AddGuid("ClosureSignedBy", aggregate.ClosureSignedBy?.Value)
            .AddString("ClosureNotes", aggregate.ClosureNotes, 400)
            .AddDateTime("ClosedAtUtc", aggregate.ClosedAtUtc?.Value)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddString("RejectedReason", aggregate.RejectedReason, 400)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_PermitWorkflow_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[PermitToWork].[usp_PermitWorkflow_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"PermitWorkflow {aggregate.PermitWorkflowId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        // Outbox wiring (platform-wide domain-event fix): enqueue any domain events raised
        // during this lifecycle transition. Previously only AddAsync did this, so every
        // post-creation transition (Approve, Issue, Reject, Archive, etc.) silently dropped
        // its domain event. See AddAsync remarks for the outbox flush consistency gap.
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <summary>Reconstructs a <see cref="PermitWorkflow"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow Hydrate(PermitWorkflowRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.PermitToWork.PermitWorkflow.Aggregates.PermitWorkflow>();

        AggregateReflectionHydrator.SetProperty(agg, "PermitWorkflowId", PermitWorkflowId.FromGuid(row.PermitWorkflowId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<PermitWorkflowStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "PermitNumber", row.PermitNumber);
        AggregateReflectionHydrator.SetProperty(agg, "PermitTypeCode", row.PermitTypeCode);
        AggregateReflectionHydrator.SetProperty(agg, "RequesterUserId", row.RequesterUserId.HasValue ? new UserId(row.RequesterUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, "IssuerUserId", row.IssuerUserId.HasValue ? new UserId(row.IssuerUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, "WorkDescription", row.WorkDescription);
        AggregateReflectionHydrator.SetProperty(agg, "RequiredControls", row.RequiredControls);
        AggregateReflectionHydrator.SetProperty(agg, "RiskScore", row.RiskScore);
        AggregateReflectionHydrator.SetProperty(agg, "ControlsConfirmedBy", row.ControlsConfirmedBy);
        AggregateReflectionHydrator.SetProperty(agg, "ControlsImplementedAtUtc", row.ControlsImplementedAtUtc.HasValue ? UtcTimestamp.From(row.ControlsImplementedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ApprovedBy", row.ApprovedBy.HasValue ? new UserId(row.ApprovedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ApprovedAtUtc", row.ApprovedAtUtc.HasValue ? UtcTimestamp.From(row.ApprovedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "IssuedAtUtc", row.IssuedAtUtc.HasValue ? UtcTimestamp.From(row.IssuedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ExpiresAtUtc", row.ExpiresAtUtc.HasValue ? UtcTimestamp.From(row.ExpiresAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ClosureSignedBy", row.ClosureSignedBy.HasValue ? new UserId(row.ClosureSignedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ClosureNotes", row.ClosureNotes);
        AggregateReflectionHydrator.SetProperty(agg, "ClosedAtUtc", row.ClosedAtUtc.HasValue ? UtcTimestamp.From(row.ClosedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "ArchivedAtUtc", row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, "RejectedReason", row.RejectedReason);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPermitWorkflowDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_PermitWorkflow_GetById</c>/<c>usp_PermitWorkflow_GetAllActive</c>.</summary>
    private sealed record PermitWorkflowRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string), populated
        /// by <c>usp_PermitWorkflow_GetById</c>. Not populated by <c>usp_PermitWorkflow_GetAllActive</c>
        /// (list projection is unchanged); mutation callers obtain the ETag from a prior
        /// single-record read or from the PermitWorkflow read-model (<c>PermitWorkflowItemDto.ETag</c>).
        /// </summary>
        public string? ETag { get; init; }

        public required Guid PermitWorkflowId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string PermitNumber { get; init; }
        public required string PermitTypeCode { get; init; }
        public Guid? RequesterUserId { get; init; }
        public Guid? IssuerUserId { get; init; }
        public required string WorkDescription { get; init; }
        public string? RequiredControls { get; init; }
        public int? RiskScore { get; init; }
        public string? ControlsConfirmedBy { get; init; }
        public DateTime? ControlsImplementedAtUtc { get; init; }
        public Guid? ApprovedBy { get; init; }
        public DateTime? ApprovedAtUtc { get; init; }
        public DateTime? IssuedAtUtc { get; init; }
        public DateTime? ExpiresAtUtc { get; init; }
        public Guid? ClosureSignedBy { get; init; }
        public string? ClosureNotes { get; init; }
        public DateTime? ClosedAtUtc { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public string? RejectedReason { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
