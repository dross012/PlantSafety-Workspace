// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Training
//  Layer:          Domain
//  File:           Training.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Training module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Training.Aggregates;

/// <summary>Aggregate root for the Training bounded context.</summary>
public sealed class Training
{
    /// <summary>Unique identity of this Training aggregate.</summary>
    public TrainingId TrainingId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<ITrainingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ITrainingDomainEvent> DomainEvents
        => new ReadOnlyCollection<ITrainingDomainEvent>(_domainEvents);

    private Training() { }

    /// <summary>Creates a new Training aggregate and raises <see cref="TrainingCreatedEvent"/>.</summary>
    public static Training Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new Training
        {
            TrainingId     = TrainingId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new TrainingCreatedEvent(
            EventId:       Uuid7.New(),
            TrainingId:      agg.TrainingId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
