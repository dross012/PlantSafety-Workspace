// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendar.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates;

/// <summary>Aggregate root for the RegulatoryCalendar bounded context.</summary>
public sealed partial class RegulatoryCalendar
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this RegulatoryCalendar aggregate.</summary>
    public RegulatoryCalendarId RegulatoryCalendarId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public RegulatoryCalendarStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IRegulatoryCalendarDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IRegulatoryCalendarDomainEvent> DomainEvents
        => new ReadOnlyCollection<IRegulatoryCalendarDomainEvent>(_domainEvents);

    // For rehydration only.
    private RegulatoryCalendar() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new RegulatoryCalendar aggregate and raises <see cref="RegulatoryCalendarCreatedEvent"/>.</summary>
    public static RegulatoryCalendar Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new RegulatoryCalendar
        {
            RegulatoryCalendarId    = RegulatoryCalendarId.New(),
            Status      = RegulatoryCalendarStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new RegulatoryCalendarCreatedEvent(
            EventId:       new RegulatoryCalendarEventId(Uuid7.New()),
            RegulatoryCalendarId:      agg.RegulatoryCalendarId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IRegulatoryCalendarDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params RegulatoryCalendarStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for RegulatoryCalendar {RegulatoryCalendarId}.");
    }
}
