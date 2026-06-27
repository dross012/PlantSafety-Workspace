// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           WorkerWellBeing.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the WorkerWellBeing module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.WorkerWellBeing.Aggregates;

/// <summary>Aggregate root for the WorkerWellBeing bounded context.</summary>
public sealed class WorkerWellBeing
{
    /// <summary>Unique identity of this WorkerWellBeing aggregate.</summary>
    public WorkerWellBeingId WorkerWellBeingId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IWorkerWellBeingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IWorkerWellBeingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IWorkerWellBeingDomainEvent>(_domainEvents);

    private WorkerWellBeing() { }

    /// <summary>Creates a new WorkerWellBeing aggregate and raises <see cref="WorkerWellBeingCreatedEvent"/>.</summary>
    public static WorkerWellBeing Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new WorkerWellBeing
        {
            WorkerWellBeingId     = WorkerWellBeingId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new WorkerWellBeingCreatedEvent(
            EventId:       Uuid7.New(),
            WorkerWellBeingId:      agg.WorkerWellBeingId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
