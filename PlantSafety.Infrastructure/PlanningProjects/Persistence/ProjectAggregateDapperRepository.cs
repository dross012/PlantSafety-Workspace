// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PlanningProjects
//  Layer:          Infrastructure
//  File:           ProjectAggregateDapperRepository.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IProjectRepository (Domain write-side
//                  repository interface for the Project aggregate root). All DB access via stored
//                  procedures in the [PlanningProjects] schema, against the
//                  [PlanningProjects].[PlanningProject] / [PlanningProjects].[PlanningProjectMilestone]
//                  tables (distinct from the pre-existing, unrelated
//                  [PlanningProjects].[Project] table backing the LIVE Features vertical, and from
//                  the pre-existing read-only [PlanningProjects].[PlanningProjects] table). Named
//                  "ProjectAggregateDapperRepository" (not "ProjectDapperRepository") to avoid any
//                  future collision with the pre-existing, unrelated
//                  Features.PlanningProjects.Projects.Repositories.ProjectDapperRepository.
//                  Aggregate reconstruction uses AggregateReflectionHydrator because Project
//                  exposes only a private "for rehydration only" constructor and private-setter
//                  properties. GetByIdAsync is tenant-scoped and UpdateAsync enforces optimistic
//                  concurrency via @ETag from day one. Both AddAsync AND UpdateAsync enqueue
//                  domain events to the outbox after their write. Mirrors
//                  PlantSafety.Infrastructure.SafetyEvents.RootCauseAnalysis.Persistence
//                  .RootCauseAnalysisAggregateDapperRepository.
//                  Additive-only evolution.
// ============================================================================
using System.Data;
using Dapper;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Domain.PlanningProjects.Entities;
using PlantSafety.Domain.PlanningProjects.Enums;
using PlantSafety.Domain.PlanningProjects.Events;
using PlantSafety.Domain.PlanningProjects.Interfaces;
using PlantSafety.Domain.PlanningProjects.ValueObjects;
using PlantSafety.Infrastructure.Persistence.Reflection;
using ProjectAggregate = PlantSafety.Domain.PlanningProjects.Aggregates.Project;

namespace PlantSafety.Infrastructure.PlanningProjects.Persistence;

/// <summary>Dapper-backed repository for the <see cref="ProjectAggregate"/> aggregate root.</summary>
public sealed class ProjectAggregateDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<ProjectAggregateDapperRepository> logger)
    : IProjectRepository
{
    private const string MilestoneTableType = "[PlanningProjects].[PlanningProjectMilestoneTableType]";

    /// <inheritdoc/>
    public async Task<ProjectAggregate?> GetByIdAsync(ProjectId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for Project {ProjectId} Tenant {TenantId}", id.Value, tenantId);

        using var conn = connectionFactory.CreateReadConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProjectId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();

        using var multi = await conn.QueryMultipleAsync(
            new CommandDefinition(
                "[PlanningProjects].[usp_PlanningProject_GetById]",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));

        var row = await multi.ReadSingleOrDefaultAsync<ProjectRow>();
        if (row is null) return null;

        var milestoneRows = (await multi.ReadAsync<ProjectMilestoneRow>()).ToList();

        return Hydrate(row, milestoneRows);
    }

    /// <inheritdoc/>
    public async Task AddAsync(ProjectAggregate aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add Project {ProjectId}", aggregate.ProjectId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProjectId", aggregate.ProjectId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Name", aggregate.Name, 200)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddDateTime("TargetCompletionUtc", aggregate.TargetCompletionUtc.Value)
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();
        parameters.Add("Milestones", BuildMilestoneTable(aggregate.Milestones).AsTableValuedParameter(MilestoneTableType));

        await commandExecutor.ExecuteAsync(conn, "[PlanningProjects].[usp_PlanningProject_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(ProjectAggregate aggregate, string? eTag, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update Project {ProjectId}", aggregate.ProjectId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("ProjectId", aggregate.ProjectId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddGuid("ActivatedBy", aggregate.ActivatedBy?.Value)
            .AddDateTime("ActivatedAtUtc", aggregate.ActivatedAtUtc?.Value)
            .AddGuid("PutOnHoldBy", aggregate.PutOnHoldBy?.Value)
            .AddDateTime("PutOnHoldAtUtc", aggregate.PutOnHoldAtUtc?.Value)
            .AddString("OnHoldReason", aggregate.OnHoldReason, 1000)
            .AddGuid("CompletedBy", aggregate.CompletedBy?.Value)
            .AddDateTime("CompletedAtUtc", aggregate.CompletedAtUtc?.Value)
            .AddGuid("CancelledBy", aggregate.CancelledBy?.Value)
            .AddDateTime("CancelledAtUtc", aggregate.CancelledAtUtc?.Value)
            .AddString("CancellationReason", aggregate.CancellationReason, 1000)
            .AddDateTime("ArchivedAtUtc", aggregate.ArchivedAtUtc?.Value)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .AddString("ETag", eTag, size: 64)
            .Build();
        parameters.Add("Milestones", BuildMilestoneTable(aggregate.Milestones).AsTableValuedParameter(MilestoneTableType));

        // Must be invoked via ExecuteScalarAsync (not ExecuteAsync/ExecuteNonQuery): the proc's
        // SET NOCOUNT ON makes ExecuteNonQuery always return -1, which would silently disable
        // the @ETag concurrency check below. See usp_PlanningProject_Update.sql remarks.
        var rowsAffected = await commandExecutor.ExecuteScalarAsync(conn, "[PlanningProjects].[usp_PlanningProject_Update]", parameters, tx, cancellationToken: cancellationToken);
        if (rowsAffected == 0)
        {
            throw new ConcurrencyConflictException(
                $"Project {aggregate.ProjectId.Value} was modified by another process or the ETag is stale.",
                Guid.Empty);
        }

        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    private static DataTable BuildMilestoneTable(IReadOnlyCollection<ProjectMilestone> milestones)
    {
        var table = new DataTable();
        table.Columns.Add("MilestoneId", typeof(Guid));
        table.Columns.Add("Name", typeof(string));
        table.Columns.Add("TargetDateUtc", typeof(DateTime));
        table.Columns.Add("IsAchieved", typeof(bool));

        foreach (var m in milestones)
        {
            table.Rows.Add(m.MilestoneId, m.Name, m.TargetDateUtc.Value, m.IsAchieved);
        }

        return table;
    }

    /// <summary>Reconstructs a <see cref="ProjectAggregate"/> aggregate from its flat read-model rows.</summary>
    private static ProjectAggregate Hydrate(ProjectRow row, List<ProjectMilestoneRow> milestoneRows)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<ProjectAggregate>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.ProjectId), ProjectId.FromGuid(row.ProjectId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.Name), row.Name);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.Status), Enum.Parse<ProjectStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.TargetCompletionUtc), UtcTimestamp.From(row.TargetCompletionUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.OwnerId), new UserId(row.OwnerId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.ActivatedBy), row.ActivatedBy.HasValue ? new UserId(row.ActivatedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.ActivatedAtUtc), row.ActivatedAtUtc.HasValue ? UtcTimestamp.From(row.ActivatedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.PutOnHoldBy), row.PutOnHoldBy.HasValue ? new UserId(row.PutOnHoldBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.PutOnHoldAtUtc), row.PutOnHoldAtUtc.HasValue ? UtcTimestamp.From(row.PutOnHoldAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.OnHoldReason), row.OnHoldReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.CompletedBy), row.CompletedBy.HasValue ? new UserId(row.CompletedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.CompletedAtUtc), row.CompletedAtUtc.HasValue ? UtcTimestamp.From(row.CompletedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.CancelledBy), row.CancelledBy.HasValue ? new UserId(row.CancelledBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.CancelledAtUtc), row.CancelledAtUtc.HasValue ? UtcTimestamp.From(row.CancelledAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.CancellationReason), row.CancellationReason);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.ArchivedAtUtc), row.ArchivedAtUtc.HasValue ? UtcTimestamp.From(row.ArchivedAtUtc.Value) : (UtcTimestamp?)null);
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(ProjectAggregate.CreatedBy), new UserId(row.CreatedBy));

        var milestones = milestoneRows
            .Select(m => new ProjectMilestone(m.MilestoneId, m.Name, UtcTimestamp.From(m.TargetDateUtc)))
            .ToList();
        foreach (var m in milestoneRows.Where(m => m.IsAchieved))
        {
            milestones.First(x => x.MilestoneId == m.MilestoneId).Achieve();
        }

        AggregateReflectionHydrator.SetField(agg, "_milestones", milestones);
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IPlanningProjectsDomainEvent>());

        return agg;
    }

    /// <summary>Flat header row shape returned by <c>usp_PlanningProject_GetById</c>'s first result set.</summary>
    private sealed record ProjectRow
    {
        /// <summary>
        /// Optimistic-concurrency token (native RowVersion projected as a hex string). Not
        /// consumed by aggregate hydration -- callers obtain the ETag for a subsequent write from
        /// a prior single-record read.
        /// </summary>
        public string? ETag { get; init; }

        public required Guid ProjectId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Name { get; init; }
        public required string Status { get; init; }
        public required DateTime TargetCompletionUtc { get; init; }
        public required Guid OwnerId { get; init; }
        public Guid? ActivatedBy { get; init; }
        public DateTime? ActivatedAtUtc { get; init; }
        public Guid? PutOnHoldBy { get; init; }
        public DateTime? PutOnHoldAtUtc { get; init; }
        public string? OnHoldReason { get; init; }
        public Guid? CompletedBy { get; init; }
        public DateTime? CompletedAtUtc { get; init; }
        public Guid? CancelledBy { get; init; }
        public DateTime? CancelledAtUtc { get; init; }
        public string? CancellationReason { get; init; }
        public DateTime? ArchivedAtUtc { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }

    /// <summary>Milestone row shape returned by <c>usp_PlanningProject_GetById</c>'s second result set.</summary>
    private sealed record ProjectMilestoneRow
    {
        public required Guid MilestoneId { get; init; }
        public required Guid ProjectId { get; init; }
        public required string Name { get; init; }
        public required DateTime TargetDateUtc { get; init; }
        public required bool IsAchieved { get; init; }
    }
}
