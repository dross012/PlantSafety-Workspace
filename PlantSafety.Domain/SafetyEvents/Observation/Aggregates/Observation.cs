// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.Observation
//  Layer:          Domain
//  File:           Observation.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.Observation.Aggregates;

/// <summary>Aggregate root for the Observation bounded context.</summary>
public sealed partial class Observation
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Observation aggregate.</summary>
    public ObservationId ObservationId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ObservationStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IObservationDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IObservationDomainEvent> DomainEvents
        => new ReadOnlyCollection<IObservationDomainEvent>(_domainEvents);

    // For rehydration only.
    private Observation() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Observation aggregate and raises <see cref="ObservationCreatedEvent"/>.</summary>
    public static Observation Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Observation
        {
            ObservationId    = ObservationId.New(),
            Status      = ObservationStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new ObservationCreatedEvent(
            EventId:       new ObservationEventId(Uuid7.New()),
            ObservationId:      agg.ObservationId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IObservationDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ObservationStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Observation {ObservationId}.");
    }
}
