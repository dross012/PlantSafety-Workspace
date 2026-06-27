// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Domain
//  File:           Governance.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Governance module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Governance.Aggregates;

/// <summary>Aggregate root for the Governance bounded context.</summary>
public sealed class Governance
{
    /// <summary>Unique identity of this Governance aggregate.</summary>
    public GovernanceId GovernanceId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IGovernanceDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IGovernanceDomainEvent> DomainEvents
        => new ReadOnlyCollection<IGovernanceDomainEvent>(_domainEvents);

    private Governance() { }

    /// <summary>Creates a new Governance aggregate and raises <see cref="GovernanceCreatedEvent"/>.</summary>
    public static Governance Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new Governance
        {
            GovernanceId     = GovernanceId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new GovernanceCreatedEvent(
            EventId:       Uuid7.New(),
            GovernanceId:      agg.GovernanceId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
