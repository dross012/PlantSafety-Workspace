// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Domain
//  File:           ScoreCards.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.ScoreCards.Aggregates;

/// <summary>Aggregate root for the ScoreCards bounded context.</summary>
public sealed partial class ScoreCards
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this ScoreCards aggregate.</summary>
    public ScoreCardsId ScoreCardsId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ScoreCardsStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IScoreCardsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IScoreCardsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IScoreCardsDomainEvent>(_domainEvents);

    // For rehydration only.
    private ScoreCards() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new ScoreCards aggregate and raises <see cref="ScoreCardsCreatedEvent"/>.</summary>
    public static ScoreCards Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new ScoreCards
        {
            ScoreCardsId    = ScoreCardsId.New(),
            Status      = ScoreCardsStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new ScoreCardsCreatedEvent(
            EventId:       new ScoreCardsEventId(Uuid7.New()),
            ScoreCardsId:      agg.ScoreCardsId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IScoreCardsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ScoreCardsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for ScoreCards {ScoreCardsId}.");
    }
}
