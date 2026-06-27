// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           Checklists.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.Aggregates;

/// <summary>Aggregate root for the Checklists bounded context.</summary>
public sealed partial class Checklists
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Checklists aggregate.</summary>
    public ChecklistsId ChecklistsId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ChecklistsStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IChecklistsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IChecklistsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IChecklistsDomainEvent>(_domainEvents);

    // For rehydration only.
    private Checklists() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Checklists aggregate and raises <see cref="ChecklistsCreatedEvent"/>.</summary>
    public static Checklists Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new Checklists
        {
            ChecklistsId    = ChecklistsId.New(),
            Status      = ChecklistsStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new ChecklistsCreatedEvent(
            EventId:       new ChecklistsEventId(Uuid7.New()),
            ChecklistsId:      agg.ChecklistsId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IChecklistsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ChecklistsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Checklists {ChecklistsId}.");
    }
}
