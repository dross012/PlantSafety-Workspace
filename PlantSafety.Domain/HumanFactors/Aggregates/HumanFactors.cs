// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors
//  Layer:          Domain
//  File:           HumanFactors.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the HumanFactors module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.Aggregates;

/// <summary>Aggregate root for the HumanFactors bounded context.</summary>
public sealed class HumanFactors
{
    /// <summary>Unique identity of this HumanFactors aggregate.</summary>
    public HumanFactorsId HumanFactorsId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IHumanFactorsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IHumanFactorsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IHumanFactorsDomainEvent>(_domainEvents);

    private HumanFactors() { }

    /// <summary>Creates a new HumanFactors aggregate and raises <see cref="HumanFactorsCreatedEvent"/>.</summary>
    public static HumanFactors Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new HumanFactors
        {
            HumanFactorsId     = HumanFactorsId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new HumanFactorsCreatedEvent(
            EventId:       Uuid7.New(),
            HumanFactorsId:      agg.HumanFactorsId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
