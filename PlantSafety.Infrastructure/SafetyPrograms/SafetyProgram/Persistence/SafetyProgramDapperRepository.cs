// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Infrastructure
//  File:           SafetyProgramDapperRepository.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper repository for the SafetyProgram aggregate root. All DB access via
//                  stored procedures in the [SafetyPrograms] schema. Goals (a simple ordered
//                  list of free-text child entities with no independent lifecycle) are
//                  flattened to a single pipe-delimited NVARCHAR(MAX) column rather than a
//                  child table, since they carry no state beyond text + order. Aggregate
//                  rehydration uses AggregateReflectionHydrator, mirroring
//                  StopWorkEventDapperRepository (StopWorkEvent exposes no public
//                  Rehydrate()/factory path either). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Aggregates;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Entities;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Events;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Interfaces;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.SafetyPrograms.SafetyProgram.Persistence;

/// <summary>
/// Dapper-backed repository for the <see cref="Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram"/>
/// aggregate. Executes stored procedures for all persistence operations.
/// </summary>
public sealed class SafetyProgramDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperCommandExecutor commandExecutor,
    DapperQueryExecutor queryExecutor,
    IOutboxWriter outboxWriter,
    ILogger<SafetyProgramDapperRepository> logger)
    : ISafetyProgramRepository
{
    private const string GoalDelimiter = "\u001F"; // Unit separator -- safe against free-text goal content.

    /// <inheritdoc/>
    public async Task<Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram?> GetByIdAsync(
        SafetyProgramId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById SafetyProgram {SafetyProgramId}", id.Value);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("SafetyProgramId", id.Value)
            .AddGuid("TenantId", tenantId)
            .Build();

        var row = await queryExecutor.QuerySingleOrDefaultAsync<SafetyProgramRow>(
            "[SafetyPrograms].[usp_SafetyProgram_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram>> ListAsync(
        Guid tenantId,
        Guid? siteId,
        SafetyProgramStatus? statusFilter,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List SafetyProgram for tenant {TenantId}, page {PageNumber}", tenantId, pageNumber);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("StatusFilter", statusFilter?.ToString(), 20)
            .AddInt("PageNumber", pageNumber)
            .AddInt("PageSize", pageSize)
            .Build();

        var rows = await queryExecutor.QueryAsync<SafetyProgramRow>(
            "[SafetyPrograms].[usp_SafetyProgram_List]", parameters, cancellationToken);

        return rows.Select(Hydrate).ToList();
    }

    /// <inheritdoc/>
    public async Task<int> CountAsync(
        Guid tenantId, Guid? siteId, SafetyProgramStatus? statusFilter, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddGuid("SiteId", siteId)
            .AddString("StatusFilter", statusFilter?.ToString(), 20)
            .Build();

        return await queryExecutor.QuerySingleOrDefaultAsync<int>(
            "[SafetyPrograms].[usp_SafetyProgram_Count]", parameters, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddAsync(
        Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Create SafetyProgram {SafetyProgramId} for tenant {TenantId}", aggregate.SafetyProgramId.Value, aggregate.TenantId);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("SafetyProgramId", aggregate.SafetyProgramId.Value)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("Name", aggregate.Name, 256)
            .AddString("Description", aggregate.Description, 4000)
            .AddString("Category", aggregate.Category.ToString(), 40)
            .AddDateTime("StartDate", aggregate.StartDate.ToDateTime(TimeOnly.MinValue))
            .AddDateTime("EndDate", aggregate.EndDate?.ToDateTime(TimeOnly.MinValue))
            .AddGuid("OwnerId", aggregate.OwnerId.Value)
            .AddDouble("ParticipationPercent", aggregate.ParticipationPercent)
            .AddInt("EnrolledCount", aggregate.EnrolledCount)
            .AddString("Goals", SerializeGoals(aggregate.Goals), 4000)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyPrograms].[usp_SafetyProgram_Create]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(
        Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update SafetyProgram {SafetyProgramId}", aggregate.SafetyProgramId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .WithCanonicalHeaders(aggregate.TenantId, Guid.NewGuid(), aggregate.CreatedBy.Value, DateTime.UtcNow)
            .AddGuid("SafetyProgramId", aggregate.SafetyProgramId.Value)
            .AddString("Status", aggregate.Status.ToString(), 20)
            .AddString("Name", aggregate.Name, 256)
            .AddString("Description", aggregate.Description, 4000)
            .AddDateTime("StartDate", aggregate.StartDate.ToDateTime(TimeOnly.MinValue))
            .AddDateTime("EndDate", aggregate.EndDate?.ToDateTime(TimeOnly.MinValue))
            .AddDouble("ParticipationPercent", aggregate.ParticipationPercent)
            .AddInt("EnrolledCount", aggregate.EnrolledCount)
            .AddString("Goals", SerializeGoals(aggregate.Goals), 4000)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[SafetyPrograms].[usp_SafetyProgram_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        foreach (var domainEvent in aggregate.DomainEvents)
        {
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());
        }
        aggregate.ClearDomainEvents();
    }

    private static string SerializeGoals(IReadOnlyList<SafetyProgramGoal> goals)
        => string.Join(GoalDelimiter, goals.OrderBy(g => g.SortOrder).Select(g => g.Text));

    private static List<SafetyProgramGoal> DeserializeGoals(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return [];

        var order = 0;
        return raw.Split(GoalDelimiter, StringSplitOptions.RemoveEmptyEntries)
            .Select(text => SafetyProgramGoal.Create(text, order++))
            .ToList();
    }

    private static Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram Hydrate(SafetyProgramRow row)
    {
        var aggregate = AggregateReflectionHydrator.CreateUninitialized<Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram>();

        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.SafetyProgramId), SafetyProgramId.FromGuid(row.SafetyProgramId));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.Status), Enum.Parse<SafetyProgramStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.Name), row.Name);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.Description), row.Description);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.Category), Enum.Parse<SafetyProgramCategory>(row.Category));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.StartDate), DateOnly.FromDateTime(row.StartDate));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.EndDate), row.EndDate.HasValue ? DateOnly.FromDateTime(row.EndDate.Value) : (DateOnly?)null);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.OwnerId), new UserId(row.OwnerId));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.ParticipationPercent), row.ParticipationPercent);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.EnrolledCount), row.EnrolledCount);
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.CreatedAtUtc), new UtcTimestamp(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(aggregate, nameof(Domain.SafetyPrograms.SafetyProgram.Aggregates.SafetyProgram.CreatedBy), new UserId(row.CreatedBy));

        AggregateReflectionHydrator.SetField(aggregate, "_goals", DeserializeGoals(row.Goals));
        AggregateReflectionHydrator.SetField(aggregate, "_domainEvents", new List<ISafetyProgramDomainEvent>());

        return aggregate;
    }

    private sealed record SafetyProgramRow
    {
        public required Guid SafetyProgramId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required string Category { get; init; }
        public required DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public required Guid OwnerId { get; init; }
        public required double ParticipationPercent { get; init; }
        public required int EnrolledCount { get; init; }
        public string? Goals { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }
}
