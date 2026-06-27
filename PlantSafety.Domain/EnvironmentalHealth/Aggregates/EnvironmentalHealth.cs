// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           EnvironmentalHealth.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the EnvironmentalHealth module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Aggregates;

/// <summary>Aggregate root for the EnvironmentalHealth bounded context.</summary>
public sealed class EnvironmentalHealth
{
    /// <summary>Unique identity of this EnvironmentalHealth aggregate.</summary>
    public EnvironmentalHealthId EnvironmentalHealthId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IEnvironmentalHealthDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEnvironmentalHealthDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEnvironmentalHealthDomainEvent>(_domainEvents);

    private EnvironmentalHealth() { }

    /// <summary>Creates a new EnvironmentalHealth aggregate and raises <see cref="EnvironmentalHealthCreatedEvent"/>.</summary>
    public static EnvironmentalHealth Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new EnvironmentalHealth
        {
            EnvironmentalHealthId     = EnvironmentalHealthId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new EnvironmentalHealthCreatedEvent(
            EventId:       Uuid7.New(),
            EnvironmentalHealthId:      agg.EnvironmentalHealthId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
