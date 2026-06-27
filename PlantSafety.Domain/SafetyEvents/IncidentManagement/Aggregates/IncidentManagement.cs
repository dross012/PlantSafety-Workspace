// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagement.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates;

/// <summary>Aggregate root for the IncidentManagement bounded context.</summary>
public sealed partial class IncidentManagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this IncidentManagement aggregate.</summary>
    public IncidentManagementId IncidentManagementId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public IncidentManagementStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IIncidentManagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IIncidentManagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<IIncidentManagementDomainEvent>(_domainEvents);

    // For rehydration only.
    private IncidentManagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new IncidentManagement aggregate and raises <see cref="IncidentManagementCreatedEvent"/>.</summary>
    public static IncidentManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new IncidentManagement
        {
            IncidentManagementId    = IncidentManagementId.New(),
            Status      = IncidentManagementStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new IncidentManagementCreatedEvent(
            EventId:       new IncidentManagementEventId(Uuid7.New()),
            IncidentManagementId:      agg.IncidentManagementId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IIncidentManagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params IncidentManagementStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for IncidentManagement {IncidentManagementId}.");
    }
}
