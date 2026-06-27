// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChange.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates;

/// <summary>Aggregate root for the ManagementOfChange bounded context.</summary>
public sealed partial class ManagementOfChange
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this ManagementOfChange aggregate.</summary>
    public ManagementOfChangeId ManagementOfChangeId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ManagementOfChangeStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IManagementOfChangeDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IManagementOfChangeDomainEvent> DomainEvents
        => new ReadOnlyCollection<IManagementOfChangeDomainEvent>(_domainEvents);

    // For rehydration only.
    private ManagementOfChange() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new ManagementOfChange aggregate and raises <see cref="ManagementOfChangeCreatedEvent"/>.</summary>
    public static ManagementOfChange Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new ManagementOfChange
        {
            ManagementOfChangeId    = ManagementOfChangeId.New(),
            Status      = ManagementOfChangeStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new ManagementOfChangeCreatedEvent(
            EventId:       new ManagementOfChangeEventId(Uuid7.New()),
            ManagementOfChangeId:      agg.ManagementOfChangeId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IManagementOfChangeDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ManagementOfChangeStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for ManagementOfChange {ManagementOfChangeId}.");
    }
}
