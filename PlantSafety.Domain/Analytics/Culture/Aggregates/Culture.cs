// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Domain
//  File:           Culture.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Culture.Aggregates;

/// <summary>Aggregate root for the Culture bounded context.</summary>
public sealed partial class Culture
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Culture aggregate.</summary>
    public CultureId CultureId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CultureStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ICultureDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICultureDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICultureDomainEvent>(_domainEvents);

    // For rehydration only.
    private Culture() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Culture aggregate and raises <see cref="CultureCreatedEvent"/>.</summary>
    public static Culture Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Culture
        {
            CultureId    = CultureId.New(),
            Status      = CultureStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new CultureCreatedEvent(
            EventId:       new CultureEventId(Uuid7.New()),
            CultureId:      agg.CultureId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICultureDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params CultureStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Culture {CultureId}.");
    }
}
