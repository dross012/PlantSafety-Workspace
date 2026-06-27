// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           Dashboard.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.Aggregates;

/// <summary>Aggregate root for the Dashboard bounded context.</summary>
public sealed partial class Dashboard
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Dashboard aggregate.</summary>
    public DashboardId DashboardId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DashboardStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IDashboardDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDashboardDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDashboardDomainEvent>(_domainEvents);

    // For rehydration only.
    private Dashboard() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Dashboard aggregate and raises <see cref="DashboardCreatedEvent"/>.</summary>
    public static Dashboard Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Dashboard
        {
            DashboardId    = DashboardId.New(),
            Status      = DashboardStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new DashboardCreatedEvent(
            EventId:       new DashboardEventId(Uuid7.New()),
            DashboardId:      agg.DashboardId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IDashboardDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params DashboardStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Dashboard {DashboardId}.");
    }
}
