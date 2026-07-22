// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Infrastructure
//  File:           AutomatedIncidentDetectionDapperRepository.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Dapper-backed implementation of IAutomatedIncidentDetectionRepository (Domain repository
//                  interface for the AutomatedIncidentDetection aggregate root). All DB access via stored
//                  procedures in the [Observability] schema. Aggregate reconstruction uses
//                  AggregateReflectionHydrator because AutomatedIncidentDetection exposes only a private
//                  'for rehydration only' constructor and private-setter properties.
//                  ListAsync evaluates the supplied specification in-memory over the
//                  full active candidate set, since the interface carries no tenant/site
//                  scoping parameter to push down to SQL.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates;
using PlantSafety.Domain.Observability.AutomatedIncidentDetection.Enums;
using PlantSafety.Domain.Observability.AutomatedIncidentDetection.Events;
using PlantSafety.Domain.Observability.AutomatedIncidentDetection.Interfaces;
using PlantSafety.Domain.Observability.AutomatedIncidentDetection.Specifications;
using PlantSafety.Domain.Observability.AutomatedIncidentDetection.ValueObjects.Ids;
using PlantSafety.Infrastructure.Persistence.Reflection;

namespace PlantSafety.Infrastructure.Observability.AutomatedIncidentDetection.Persistence;

/// <summary>Dapper-backed repository for the <see cref="AutomatedIncidentDetection"/> aggregate root.</summary>
public sealed class AutomatedIncidentDetectionDapperRepository(
    ISqlConnectionFactory connectionFactory,
    DapperQueryExecutor queryExecutor,
    DapperCommandExecutor commandExecutor,
    IOutboxWriter outboxWriter,
    ILogger<AutomatedIncidentDetectionDapperRepository> logger)
    : IAutomatedIncidentDetectionRepository
{
    /// <inheritdoc/>
    public async Task<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection?> GetByIdAsync(AutomatedIncidentDetectionId id, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("GetById for AutomatedIncidentDetection {AutomatedIncidentDetectionId}", id.Value);

        var parameters = new StoredProcedureBuilder().AddGuid("AutomatedIncidentDetectionId", id.Value).Build();
        var row = await queryExecutor.QuerySingleOrDefaultAsync<AutomatedIncidentDetectionRow>(
            "[Observability].[usp_AutomatedIncidentDetection_GetById]", parameters, cancellationToken);

        return row is null ? null : Hydrate(row);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection>> ListAsync(AutomatedIncidentDetectionSpecification specification, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("List AutomatedIncidentDetection via specification {Specification}", specification.GetType().Name);

        var rows = await queryExecutor.QueryAsync<AutomatedIncidentDetectionRow>(
            "[Observability].[usp_AutomatedIncidentDetection_GetAllActive]", new DynamicParameters(), cancellationToken);

        var predicate = specification.ToPredicate();
        var results = new List<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection>();

        foreach (var row in rows)
        {
            var aggregate = Hydrate(row);
            if (predicate(aggregate))
                results.Add(aggregate);
        }

        return results;
    }

    /// <inheritdoc/>
    public async Task AddAsync(PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Add AutomatedIncidentDetection {AutomatedIncidentDetectionId}", aggregate.AutomatedIncidentDetectionId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AutomatedIncidentDetectionId", aggregate.AutomatedIncidentDetectionId.Value)
            .AddGuid("TenantId", aggregate.TenantId)
            .AddGuid("SiteId", aggregate.SiteId)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("DetectionType", aggregate.DetectionType, 400)
            .AddString("LocationDescription", aggregate.LocationDescription, 400)
            .AddGuid("NotifiedSupervisorUserId", aggregate.NotifiedSupervisorUserId?.Value)
            .AddGuid("AcknowledgedBy", aggregate.AcknowledgedBy?.Value)
            .AddGuid("IncidentReferenceId", aggregate.IncidentReferenceId)
            .AddString("DismissalReason", aggregate.DismissalReason, 400)
            .AddDateTime("CreatedAtUtc", aggregate.CreatedAtUtc.Value)
            .AddGuid("CreatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Observability].[usp_AutomatedIncidentDetection_Insert]", parameters, tx, cancellationToken: cancellationToken);
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
    public async Task UpdateAsync(PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection aggregate, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Update AutomatedIncidentDetection {AutomatedIncidentDetectionId}", aggregate.AutomatedIncidentDetectionId.Value);

        using var conn = connectionFactory.CreateConnection();
        using var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted);

        var parameters = new StoredProcedureBuilder()
            .AddGuid("AutomatedIncidentDetectionId", aggregate.AutomatedIncidentDetectionId.Value)
            .AddString("Status", aggregate.Status.ToString(), 50)
            .AddString("DetectionType", aggregate.DetectionType, 400)
            .AddString("LocationDescription", aggregate.LocationDescription, 400)
            .AddGuid("NotifiedSupervisorUserId", aggregate.NotifiedSupervisorUserId?.Value)
            .AddGuid("AcknowledgedBy", aggregate.AcknowledgedBy?.Value)
            .AddGuid("IncidentReferenceId", aggregate.IncidentReferenceId)
            .AddString("DismissalReason", aggregate.DismissalReason, 400)
            .AddGuid("UpdatedBy", aggregate.CreatedBy.Value)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Observability].[usp_AutomatedIncidentDetection_Update]", parameters, tx, cancellationToken: cancellationToken);
        tx.Commit();
    }

    /// <summary>Reconstructs a <see cref="AutomatedIncidentDetection"/> aggregate from its flat read-model row.</summary>
    private static PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection Hydrate(AutomatedIncidentDetectionRow row)
    {
        var agg = AggregateReflectionHydrator.CreateUninitialized<PlantSafety.Domain.Observability.AutomatedIncidentDetection.Aggregates.AutomatedIncidentDetection>();

        AggregateReflectionHydrator.SetProperty(agg, "AutomatedIncidentDetectionId", AutomatedIncidentDetectionId.FromGuid(row.AutomatedIncidentDetectionId));
        AggregateReflectionHydrator.SetProperty(agg, "TenantId", row.TenantId);
        AggregateReflectionHydrator.SetProperty(agg, "SiteId", row.SiteId);
        AggregateReflectionHydrator.SetProperty(agg, "Status", Enum.Parse<AutomatedIncidentDetectionStatus>(row.Status));
        AggregateReflectionHydrator.SetProperty(agg, "DetectionType", row.DetectionType);
        AggregateReflectionHydrator.SetProperty(agg, "LocationDescription", row.LocationDescription);
        AggregateReflectionHydrator.SetProperty(agg, "NotifiedSupervisorUserId", row.NotifiedSupervisorUserId.HasValue ? new UserId(row.NotifiedSupervisorUserId.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, "AcknowledgedBy", row.AcknowledgedBy.HasValue ? new UserId(row.AcknowledgedBy.Value) : (UserId?)null);
        AggregateReflectionHydrator.SetProperty(agg, "IncidentReferenceId", row.IncidentReferenceId);
        AggregateReflectionHydrator.SetProperty(agg, "DismissalReason", row.DismissalReason);
        AggregateReflectionHydrator.SetProperty(agg, "CreatedAtUtc", UtcTimestamp.From(row.CreatedAtUtc));
        AggregateReflectionHydrator.SetProperty(agg, "CreatedBy", new UserId(row.CreatedBy));
        AggregateReflectionHydrator.SetField(agg, "_domainEvents", new List<IAutomatedIncidentDetectionDomainEvent>());

        return agg;
    }

    /// <summary>Flat read-model row shape returned by <c>usp_AutomatedIncidentDetection_GetById</c>/<c>usp_AutomatedIncidentDetection_GetAllActive</c>.</summary>
    private sealed record AutomatedIncidentDetectionRow
    {
        public required Guid AutomatedIncidentDetectionId { get; init; }
        public required Guid TenantId { get; init; }
        public Guid? SiteId { get; init; }
        public required string Status { get; init; }
        public required string DetectionType { get; init; }
        public required string LocationDescription { get; init; }
        public Guid? NotifiedSupervisorUserId { get; init; }
        public Guid? AcknowledgedBy { get; init; }
        public Guid? IncidentReferenceId { get; init; }
        public string? DismissalReason { get; init; }
        public required DateTime CreatedAtUtc { get; init; }
        public required Guid CreatedBy { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public Guid? UpdatedBy { get; init; }
    }
}
