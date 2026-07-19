// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagement.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  incident-lifecycle record shape grounded in
//                  project_plantsafety_domains.md Module 10 (Incident Management:
//                  incident capture, investigation, RCA, corrective actions,
//                  flagged as a Critical Module; KPIs: MTTR, incident severity
//                  distribution) and ui_incident_detail_wireframe.md (Severity,
//                  Category/Subcategory, Equipment/Location, Description, Root Cause
//                  Panel, Resolution Panel fields), cross-checked against the sibling
//                  CreateIncidentCommand shape already used by the parallel
//                  Application-layer Incident slice (title, description, severity,
//                  location, reportedByWorkerId, assignedInvestigatorId).
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the IncidentManagement bounded context. Tracks a
//                  single safety incident: its title, description, severity,
//                  location, reporter/investigator, injury classification, root
//                  cause, corrective actions, and closure.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates;

/// <summary>Aggregate root for the IncidentManagement bounded context.</summary>
public sealed partial class IncidentManagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this IncidentManagement aggregate.</summary>
    public IncidentManagementId IncidentManagementId { get; private set; }

    /// <summary>Tenant that owns this incident record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the incident occurred at. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public IncidentManagementStatus Status { get; private set; }

    // ---- Incident-specific business properties -----------------------------------

    /// <summary>Human-readable incident title/summary.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Narrative description of what happened.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Incident severity: Low, Medium, High, Critical.</summary>
    public string Severity { get; private set; } = "Low";

    /// <summary>Physical location/equipment where the incident occurred.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the incident actually occurred (may differ from CreatedAtUtc).</summary>
    public UtcTimestamp IncidentOccurredAtUtc { get; private set; }

    /// <summary>Injury classification, when personnel injury occurred (e.g., None, FirstAid, MedicalTreatment, LostTime, Fatality).</summary>
    public string InjuryType { get; private set; } = "None";

    /// <summary>Identity of the worker who reported the incident.</summary>
    public UserId ReportedByWorkerId { get; private set; }

    /// <summary>Identity of the investigator assigned to this incident, if assigned.</summary>
    public UserId? AssignedInvestigatorId { get; private set; }

    /// <summary>Root cause identified during investigation/RCA, once determined.</summary>
    public string RootCause { get; private set; } = string.Empty;

    /// <summary>Corrective actions identified/implemented as a result of this incident.</summary>
    public string CorrectiveActions { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the incident was closed, once resolved.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

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
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string title = "",
        string description = "",
        string severity = "Low",
        string location = "",
        UtcTimestamp? incidentOccurredAtUtc = null,
        string injuryType = "None",
        UserId reportedByWorkerId = default,
        UserId? assignedInvestigatorId = null)
    {
        var agg = new IncidentManagement
        {
            IncidentManagementId     = IncidentManagementId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            Status                   = IncidentManagementStatus.Created,
            Title                    = (title ?? string.Empty).Trim(),
            Description              = (description ?? string.Empty).Trim(),
            Severity                 = (severity ?? "Low").Trim(),
            Location                 = (location ?? string.Empty).Trim(),
            IncidentOccurredAtUtc    = incidentOccurredAtUtc ?? createdAtUtc,
            InjuryType               = (injuryType ?? "None").Trim(),
            ReportedByWorkerId       = reportedByWorkerId,
            AssignedInvestigatorId   = assignedInvestigatorId,
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
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
