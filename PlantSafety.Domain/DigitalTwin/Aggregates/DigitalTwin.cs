// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Domain
//  File:           DigitalTwin.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the DigitalTwin module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.DigitalTwin.Aggregates;

/// <summary>Aggregate root for the DigitalTwin bounded context.</summary>
public sealed class DigitalTwin
{
    /// <summary>Unique identity of this DigitalTwin aggregate.</summary>
    public DigitalTwinId DigitalTwinId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IDigitalTwinDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDigitalTwinDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDigitalTwinDomainEvent>(_domainEvents);

    private DigitalTwin() { }

    /// <summary>Creates a new DigitalTwin aggregate and raises <see cref="DigitalTwinCreatedEvent"/>.</summary>
    public static DigitalTwin Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new DigitalTwin
        {
            DigitalTwinId     = DigitalTwinId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new DigitalTwinCreatedEvent(
            EventId:       Uuid7.New(),
            DigitalTwinId:      agg.DigitalTwinId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
