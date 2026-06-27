// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Domain
//  File:           AiSafetyIntelligence.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the AiSafetyIntelligence module.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AiSafetyIntelligence.Aggregates;

/// <summary>Aggregate root for the AiSafetyIntelligence bounded context.</summary>
public sealed class AiSafetyIntelligence
{
    /// <summary>Unique identity of this AiSafetyIntelligence aggregate.</summary>
    public AiSafetyIntelligenceId AiSafetyIntelligenceId { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // Domain events
    private readonly List<IAiSafetyIntelligenceDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IAiSafetyIntelligenceDomainEvent> DomainEvents
        => new ReadOnlyCollection<IAiSafetyIntelligenceDomainEvent>(_domainEvents);

    private AiSafetyIntelligence() { }

    /// <summary>Creates a new AiSafetyIntelligence aggregate and raises <see cref="AiSafetyIntelligenceCreatedEvent"/>.</summary>
    public static AiSafetyIntelligence Create(
        UserId createdBy, UtcTimestamp createdAtUtc,
        CorrelationId correlationId, CausationId causationId, PerformedByActor actor)
    {
        var agg = new AiSafetyIntelligence
        {
            AiSafetyIntelligenceId     = AiSafetyIntelligenceId.New(),
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg._domainEvents.Add(new AiSafetyIntelligenceCreatedEvent(
            EventId:       Uuid7.New(),
            AiSafetyIntelligenceId:      agg.AiSafetyIntelligenceId.Value,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
