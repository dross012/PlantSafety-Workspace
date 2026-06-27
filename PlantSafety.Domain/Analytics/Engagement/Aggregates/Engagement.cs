// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Domain
//  File:           Engagement.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Engagement.Aggregates;

/// <summary>Aggregate root for the Engagement bounded context.</summary>
public sealed partial class Engagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Engagement aggregate.</summary>
    public EngagementId EngagementId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EngagementStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEngagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEngagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEngagementDomainEvent>(_domainEvents);

    // For rehydration only.
    private Engagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Engagement aggregate and raises <see cref="EngagementCreatedEvent"/>.</summary>
    public static Engagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Engagement
        {
            EngagementId    = EngagementId.New(),
            Status      = EngagementStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new EngagementCreatedEvent(
            EventId:       new EngagementEventId(Uuid7.New()),
            EngagementId:      agg.EngagementId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEngagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EngagementStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Engagement {EngagementId}.");
    }
}
