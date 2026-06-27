// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagement.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates;

/// <summary>Aggregate root for the EmergencyManagement bounded context.</summary>
public sealed partial class EmergencyManagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this EmergencyManagement aggregate.</summary>
    public EmergencyManagementId EmergencyManagementId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EmergencyManagementStatus Status { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IEmergencyManagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IEmergencyManagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<IEmergencyManagementDomainEvent>(_domainEvents);

    // For rehydration only.
    private EmergencyManagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new EmergencyManagement aggregate and raises <see cref="EmergencyManagementCreatedEvent"/>.</summary>
    public static EmergencyManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
    {
        var agg = new EmergencyManagement
        {
            EmergencyManagementId    = EmergencyManagementId.New(),
            Status      = EmergencyManagementStatus.Created,
            CreatedAtUtc = createdAtUtc,
            CreatedBy    = createdBy,
        };

        agg.AddDomainEvent(new EmergencyManagementCreatedEvent(
            EventId:       new EmergencyManagementEventId(Uuid7.New()),
            EmergencyManagementId:      agg.EmergencyManagementId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IEmergencyManagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params EmergencyManagementStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for EmergencyManagement {EmergencyManagementId}.");
    }
}
