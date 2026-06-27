// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Domain
//  File:           EnterpriseFabric.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the EnterpriseFabric module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnterpriseFabric.Aggregates;

/// <summary>Aggregate root for the EnterpriseFabric bounded context.</summary>
public sealed class EnterpriseFabric
{
    /// <summary>Unique identity of this EnterpriseFabric aggregate.</summary>
    public EnterpriseFabricId EnterpriseFabricId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IEnterpriseFabricDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEnterpriseFabricDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEnterpriseFabricDomainEvent>(_domainEvents);

    private EnterpriseFabric() { }

    /// <summary>Creates a new EnterpriseFabric aggregate and raises <see cref="EnterpriseFabricCreatedEvent"/>.</summary>
    public static EnterpriseFabric Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new EnterpriseFabric
        {
            EnterpriseFabricId     = EnterpriseFabricId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new EnterpriseFabricCreatedEvent(
            EventId:       Uuid7.New(),
            EnterpriseFabricId:      agg.EnterpriseFabricId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
