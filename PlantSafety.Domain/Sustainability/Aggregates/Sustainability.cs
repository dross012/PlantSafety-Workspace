// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Domain
//  File:           Sustainability.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Sustainability module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.Aggregates;

/// <summary>Aggregate root for the Sustainability bounded context.</summary>
public sealed class Sustainability
{
    /// <summary>Unique identity of this Sustainability aggregate.</summary>
    public SustainabilityId SustainabilityId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<ISustainabilityDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISustainabilityDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISustainabilityDomainEvent>(_domainEvents);

    private Sustainability() { }

    /// <summary>Creates a new Sustainability aggregate and raises <see cref="SustainabilityCreatedEvent"/>.</summary>
    public static Sustainability Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new Sustainability
        {
            SustainabilityId     = SustainabilityId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new SustainabilityCreatedEvent(
            EventId:       Uuid7.New(),
            SustainabilityId:      agg.SustainabilityId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
