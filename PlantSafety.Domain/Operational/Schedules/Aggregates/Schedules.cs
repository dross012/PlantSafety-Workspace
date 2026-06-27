// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           Schedules.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.Aggregates;

/// <summary>Aggregate root for the Schedules bounded context.</summary>
public sealed partial class Schedules
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Schedules aggregate.</summary>
    public SchedulesId SchedulesId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public SchedulesStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ISchedulesDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISchedulesDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISchedulesDomainEvent>(_domainEvents);

    // For rehydration only.
    private Schedules() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Schedules aggregate and raises <see cref="SchedulesCreatedEvent"/>.</summary>
    public static Schedules Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Schedules
        {
            SchedulesId    = SchedulesId.New(),
            Status      = SchedulesStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new SchedulesCreatedEvent(
            EventId:       new SchedulesEventId(Uuid7.New()),
            SchedulesId:      agg.SchedulesId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ISchedulesDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params SchedulesStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Schedules {SchedulesId}.");
    }
}
