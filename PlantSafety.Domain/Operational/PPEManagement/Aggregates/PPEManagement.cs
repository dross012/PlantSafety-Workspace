// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           PPEManagement.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.Aggregates;

/// <summary>Aggregate root for the PPEManagement bounded context.</summary>
public sealed partial class PPEManagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PPEManagement aggregate.</summary>
    public PPEManagementId PPEManagementId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PPEManagementStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPPEManagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPPEManagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPPEManagementDomainEvent>(_domainEvents);

    // For rehydration only.
    private PPEManagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PPEManagement aggregate and raises <see cref="PPEManagementCreatedEvent"/>.</summary>
    public static PPEManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new PPEManagement
        {
            PPEManagementId    = PPEManagementId.New(),
            Status      = PPEManagementStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new PPEManagementCreatedEvent(
            EventId:       new PPEManagementEventId(Uuid7.New()),
            PPEManagementId:      agg.PPEManagementId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPPEManagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PPEManagementStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PPEManagement {PPEManagementId}.");
    }
}
