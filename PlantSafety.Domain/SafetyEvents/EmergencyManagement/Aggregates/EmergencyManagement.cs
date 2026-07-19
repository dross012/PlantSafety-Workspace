// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagement.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  emergency-response record shape grounded in
//                  project_plantsafety_domains.md Module 9 (Emergency: "Emergency
//                  declaration, response coordination, stabilization, and closure",
//                  flagged as the highest business-continuity-priority Critical
//                  Module). Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the EmergencyManagement bounded context. Tracks
//                  a single emergency event: its type, severity, location, casualty
//                  count, notification state, and stabilization/resolution timeline.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.Aggregates;

/// <summary>Aggregate root for the EmergencyManagement bounded context.</summary>
public sealed partial class EmergencyManagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this EmergencyManagement aggregate.</summary>
    public EmergencyManagementId EmergencyManagementId { get; private set; }

    /// <summary>Tenant that owns this emergency record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the emergency occurred at. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public EmergencyManagementStatus Status { get; private set; }

    // ---- Emergency-specific business properties ---------------------------------

    /// <summary>Emergency classification (e.g., Fire, ChemicalRelease, MedicalEmergency, Evacuation, SevereWeather).</summary>
    public string EmergencyType { get; private set; } = string.Empty;

    /// <summary>Emergency severity: Minor, Moderate, Major, Catastrophic.</summary>
    public string Severity { get; private set; } = "Minor";

    /// <summary>Physical location the emergency was declared at.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>Count of casualties/injuries associated with this emergency.</summary>
    public int CasualtyCount { get; private set; }

    /// <summary>Indicates whether the emergency response team has been notified.</summary>
    public bool ResponseTeamNotified { get; private set; }

    /// <summary>UTC timestamp the emergency was declared.</summary>
    public UtcTimestamp DeclaredAtUtc { get; private set; }

    /// <summary>UTC timestamp the emergency was stabilized.</summary>
    public UtcTimestamp? StabilizedAtUtc { get; private set; }

    /// <summary>UTC timestamp the emergency was resolved/closed.</summary>
    public UtcTimestamp? ResolvedAtUtc { get; private set; }

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
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string emergencyType = "",
        string severity = "Minor",
        string location = "",
        int casualtyCount = 0,
        bool responseTeamNotified = false)
    {
        var agg = new EmergencyManagement
        {
            EmergencyManagementId  = EmergencyManagementId.New(),
            TenantId               = tenantId,
            SiteId                 = siteId,
            Status                 = EmergencyManagementStatus.Created,
            EmergencyType          = (emergencyType ?? string.Empty).Trim(),
            Severity               = (severity ?? "Minor").Trim(),
            Location               = (location ?? string.Empty).Trim(),
            CasualtyCount          = casualtyCount < 0 ? 0 : casualtyCount,
            ResponseTeamNotified   = responseTeamNotified,
            DeclaredAtUtc          = createdAtUtc,
            CreatedAtUtc           = createdAtUtc,
            CreatedBy              = createdBy,
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
