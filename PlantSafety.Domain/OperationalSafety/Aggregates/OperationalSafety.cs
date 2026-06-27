// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Domain
//  File:           OperationalSafety.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the OperationalSafety module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.OperationalSafety.Aggregates;

/// <summary>Aggregate root for the OperationalSafety bounded context.</summary>
public sealed class OperationalSafety
{
    /// <summary>Unique identity of this OperationalSafety aggregate.</summary>
    public OperationalSafetyId OperationalSafetyId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IOperationalSafetyDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IOperationalSafetyDomainEvent> DomainEvents
        => new ReadOnlyCollection<IOperationalSafetyDomainEvent>(_domainEvents);

    private OperationalSafety() { }

    /// <summary>Creates a new OperationalSafety aggregate and raises <see cref="OperationalSafetyCreatedEvent"/>.</summary>
    public static OperationalSafety Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new OperationalSafety
        {
            OperationalSafetyId     = OperationalSafetyId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new OperationalSafetyCreatedEvent(
            EventId:       Uuid7.New(),
            OperationalSafetyId:      agg.OperationalSafetyId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
