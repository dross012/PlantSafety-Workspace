// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Domain
//  File:           AiGovernance.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the AiGovernance module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiGovernance.Aggregates;

/// <summary>Aggregate root for the AiGovernance bounded context.</summary>
public sealed class AiGovernance
{
    /// <summary>Unique identity of this AiGovernance aggregate.</summary>
    public AiGovernanceId AiGovernanceId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IAiGovernanceDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAiGovernanceDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAiGovernanceDomainEvent>(_domainEvents);

    private AiGovernance() { }

    /// <summary>Creates a new AiGovernance aggregate and raises <see cref="AiGovernanceCreatedEvent"/>.</summary>
    public static AiGovernance Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new AiGovernance
        {
            AiGovernanceId     = AiGovernanceId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new AiGovernanceCreatedEvent(
            EventId:       Uuid7.New(),
            AiGovernanceId:      agg.AiGovernanceId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
