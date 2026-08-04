// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CyberPhysicalSecurity
//  Layer:          Infrastructure
//  File:           SabotageIndicatorDapperRepository.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed repository for the SabotageIndicator aggregate root. Implements
//                  both the Domain write-side repository and the Application read-side repository.
// ============================================================================
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Models;
using PlantSafety.Application.CyberPhysicalSecurity.SabotageIndicator.Repositories;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Aggregates;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Enums;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Events;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Interfaces;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.Policies;
using PlantSafety.Domain.CyberPhysicalSecurity.SabotageIndicator.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;
using PlantSafety.Infrastructure.Persistence.UnitOfWork;

namespace PlantSafety.Infrastructure.CyberPhysicalSecurity.Persistence;

/// <summary>Dapper-backed repository for the <see cref="SabotageIndicator"/> aggregate root.</summary>
public sealed class SabotageIndicatorDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<SabotageIndicatorDapperRepository> logger)
    : ISabotageIndicatorRepository, ISabotageIndicatorReadRepository
{
    /// <inheritdoc/>
    public async Task<SabotageIndicator?> GetByIdAsync(SabotageIndicatorId id, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("SabotageIndicatorId", id.Value).AddGuid("TenantId", tenantId).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<SabotageIndicatorRow>(
            "[CyberPhysicalSecurity].[usp_SabotageIndicator_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SabotageIndicator>> ListActiveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<SabotageIndicatorRow>("[CyberPhysicalSecurity].[usp_SabotageIndicator_GetActive]", parameters, cancellationToken);

        return rows.Select(Hydrate).ToList();
    }

    /// <inheritdoc/>
    async Task<IReadOnlyList<SabotageIndicatorDto>> ISabotageIndicatorReadRepository.ListActiveAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var parameters = new StoredProcedureBuilder().AddGuid("TenantId", tenantId).Build();
        var rows = await queryExecutor.QueryAsync<SabotageIndicatorRow>("[CyberPhysicalSecurity].[usp_SabotageIndicator_GetActive]", parameters, cancellationToken);

        return rows.Select(ToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SabotageIndicator>> ListForCorrelationWindowAsync(Guid tenantId, DateTime windowStartUtc, DateTime windowEndUtc, CancellationToken cancellationToken = default)
    {
        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantId", tenantId)
            .AddDateTime("WindowStartUtc", windowStartUtc)
            .AddDateTime("WindowEndUtc", windowEndUtc)
            .Build();
        var rows = await queryExecutor.QueryAsync<SabotageIndicatorRow>("[CyberPhysicalSecurity].[usp_SabotageIndicator_GetForCorrelationWindow]", parameters, cancellationToken);

        return rows.Select(Hydrate).ToList();
    }

    /// <inheritdoc/>
    public async Task AddAsync(SabotageIndicator aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add SabotageIndicator {SabotageIndicatorId}", aggregate.SabotageIndicatorId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = BuildParameters(aggregate);
        await commandExecutor.ExecuteAsync(conn, "[CyberPhysicalSecurity].[usp_SabotageIndicator_Insert]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        FlushDomainEvents(aggregate);
    }

    /// <inheritdoc/>
    public async Task UpdateAsync(SabotageIndicator aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update SabotageIndicator {SabotageIndicatorId}", aggregate.SabotageIndicatorId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = BuildParameters(aggregate);
        await commandExecutor.ExecuteAsync(conn, "[CyberPhysicalSecurity].[usp_SabotageIndicator_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();

        FlushDomainEvents(aggregate);
    }

    private static DynamicParameters BuildParameters(SabotageIndicator aggregate) => new StoredProcedureBuilder()
        .AddGuid("SabotageIndicatorId", aggregate.SabotageIndicatorId.Value)
        .AddGuid("TenantId", aggregate.TenantId)
        .AddGuid("SiteId", aggregate.SiteId)
        .AddString("Source", aggregate.Source.ToString(), 30)
        .AddGuid("SourceReferenceId", aggregate.SourceReferenceId)
        .AddString("Description", aggregate.Description, 2000)
        .AddString("Severity", aggregate.Severity.ToString(), 20)
        .AddString("Status", aggregate.Status.ToString(), 30)
        .AddInt("CorrelatedIndicatorCount", aggregate.CorrelatedIndicatorCount)
        .AddDateTime("RaisedAtUtc", aggregate.RaisedAtUtc.Value)
        .AddString("RiskManagementPriority", SabotageRiskFeedPolicy.ResolveRiskManagementPriority(aggregate.Severity), 5)
        .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
        .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
        .Build();

    private void FlushDomainEvents(SabotageIndicator aggregate)
    {
        foreach (var domainEvent in aggregate.DomainEvents)
            outboxWriter.Enqueue(domainEvent, aggregate.TenantId.ToString(), "system", Guid.Empty.ToString());

        aggregate.ClearDomainEvents();
    }

    private static SabotageIndicator Hydrate(SabotageIndicatorRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<SabotageIndicator>();

        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.SabotageIndicatorId), SabotageIndicatorId.FromGuid(row.SabotageIndicatorId));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.TenantId), row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.SiteId), row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.Source), Enum.Parse<SabotageIndicatorSource>(row.Source));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.SourceReferenceId), row.SourceReferenceId);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.Description), row.Description);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.Severity), Enum.Parse<SabotageIndicatorSeverity>(row.Severity));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.Status), Enum.Parse<SabotageIndicatorStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.CorrelatedIndicatorCount), row.CorrelatedIndicatorCount);
        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.RaisedAtUtc), UtcTimestamp.From(row.RaisedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.CreatedAtUtc), UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, nameof(SabotageIndicator.CreatedBy), new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<ISabotageIndicatorDomainEvent>());

        return agg;
    }

    private static SabotageIndicatorDto ToDto(SabotageIndicatorRow row) => new(
        row.SabotageIndicatorId, row.TenantId, row.SiteId, row.Source, row.SourceReferenceId, row.Description,
        row.Severity, row.Status, row.CorrelatedIndicatorCount, row.RaisedAtUtc, row.RiskManagementPriority);

    /// <summary>Flat read-model row shape returned by the SabotageIndicator stored procedures.</summary>
    private sealed record SabotageIndicatorRow
    {
        public required Guid SabotageIndicatorId { get; init; }
        public required Guid TenantId { get; init; }
        public required Guid SiteId { get; init; }
        public required string Source { get; init; }
        public Guid? SourceReferenceId { get; init; }
        public required string Description { get; init; }
        public required string Severity { get; init; }
        public required string Status { get; init; }
        public required int CorrelatedIndicatorCount { get; init; }
        public required DateTime RaisedAtUtc { get; init; }
        public required string RiskManagementPriority { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
    }
}
