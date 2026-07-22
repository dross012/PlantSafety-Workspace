// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReporting.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single routine safety audit / incident-reporting cycle: floor
//                  walkthroughs, near-miss reports, or incident investigations. Models the real
//                  lifecycle: scheduling, conduct (findings/no-findings outcome), corrective action,
//                  and closure.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Aggregates;

/// <summary>Aggregate root for a single routine safety audit / incident-reporting cycle.</summary>
public sealed partial class RoutineSafetyAuditsIncidentReporting
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this RoutineSafetyAuditsIncidentReporting aggregate.</summary>
    public RoutineSafetyAuditsIncidentReportingId RoutineSafetyAuditsIncidentReportingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public RoutineSafetyAuditsIncidentReportingStatus Status { get; private set; }

    // ---- Audit identity ---------------------------------------------------------

    /// <summary>Human-readable title of this audit/report.</summary>
    public string AuditTitle { get; private set; } = string.Empty;

    /// <summary>Reporting category.</summary>
    public SafetyReportingCategory ReportingCategory { get; private set; }

    /// <summary>Facility area covered.</summary>
    public string FacilityArea { get; private set; } = string.Empty;

    /// <summary>Identity of the employee who conducted the audit/walkthrough.</summary>
    public UserId ConductedByUserId { get; private set; }

    /// <summary>UTC timestamp the audit was conducted, if any.</summary>
    public UtcTimestamp? ConductedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next audit cycle is due.</summary>
    public UtcTimestamp? NextAuditDueAtUtc { get; private set; }

    /// <summary>Number of findings/hazards/deficiencies logged.</summary>
    public int FindingCount { get; private set; }

    /// <summary>Compliance score (0-100) from the audit.</summary>
    public decimal? ComplianceScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IRoutineSafetyAuditsIncidentReportingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IRoutineSafetyAuditsIncidentReportingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IRoutineSafetyAuditsIncidentReportingDomainEvent>(_domainEvents);

    // For rehydration only.
    private RoutineSafetyAuditsIncidentReporting() { }

    // ---- Factory: Create (Schedule) -----------------------------------------------------------

    /// <summary>Schedules a new routine audit/walkthrough/report cycle and raises <see cref="RoutineSafetyAuditScheduledEvent"/>.</summary>
    public static RoutineSafetyAuditsIncidentReporting Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string auditTitle = "",
        SafetyReportingCategory reportingCategory = SafetyReportingCategory.FloorWalkthrough,
        string facilityArea = "")
    {
        var agg = new RoutineSafetyAuditsIncidentReporting
        {
            RoutineSafetyAuditsIncidentReportingId = RoutineSafetyAuditsIncidentReportingId.New(),
            TenantId          = tenantId,
            SiteId            = siteId,
            Status            = RoutineSafetyAuditsIncidentReportingStatus.Scheduled,
            AuditTitle        = (auditTitle ?? string.Empty).Trim(),
            ReportingCategory = reportingCategory,
            FacilityArea      = (facilityArea ?? string.Empty).Trim(),
            CreatedAtUtc      = createdAtUtc,
            CreatedBy         = createdBy,
        };

        agg.AddDomainEvent(new RoutineSafetyAuditScheduledEvent(
            RoutineSafetyAuditsIncidentReportingEventId.New(), agg.RoutineSafetyAuditsIncidentReportingId, reportingCategory, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: Conduct -----------------------------------------------

    /// <summary>Records the conduct of the scheduled audit/walkthrough/report.</summary>
    public void Conduct(int findingCount, decimal complianceScore, UserId conductedByUserId, UtcTimestamp conductedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(RoutineSafetyAuditsIncidentReportingStatus.Scheduled);

        if (findingCount < 0)
            throw new ArgumentOutOfRangeException(nameof(findingCount), "Finding count cannot be negative.");
        if (complianceScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(complianceScore), "Compliance score must be between 0 and 100.");

        FindingCount        = findingCount;
        ComplianceScore     = complianceScore;
        ConductedByUserId   = conductedByUserId;
        ConductedAtUtc      = conductedAtUtc;

        if (findingCount == 0)
        {
            Status = RoutineSafetyAuditsIncidentReportingStatus.Conducted;
            AddDomainEvent(new RoutineSafetyAuditConductedEvent(
                RoutineSafetyAuditsIncidentReportingEventId.New(), RoutineSafetyAuditsIncidentReportingId, complianceScore, conductedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            Status = RoutineSafetyAuditsIncidentReportingStatus.FindingsLogged;
            AddDomainEvent(new RoutineSafetyAuditFindingsLoggedEvent(
                RoutineSafetyAuditsIncidentReportingEventId.New(), RoutineSafetyAuditsIncidentReportingId, findingCount, conductedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: BeginCorrectiveAction -----------------------------------------------

    /// <summary>Begins corrective action to remediate logged findings.</summary>
    public void BeginCorrectiveAction(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(RoutineSafetyAuditsIncidentReportingStatus.FindingsLogged);

        Status = RoutineSafetyAuditsIncidentReportingStatus.CorrectiveActionInProgress;

        AddDomainEvent(new RoutineSafetyAuditCorrectiveActionStartedEvent(
            RoutineSafetyAuditsIncidentReportingEventId.New(), RoutineSafetyAuditsIncidentReportingId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Close -----------------------------------------------------------

    /// <summary>Closes out this audit cycle and schedules the next one.</summary>
    public void Close(UtcTimestamp nextDueAtUtc, UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(RoutineSafetyAuditsIncidentReportingStatus.Conducted, RoutineSafetyAuditsIncidentReportingStatus.CorrectiveActionInProgress);

        NextAuditDueAtUtc = nextDueAtUtc;
        Status = RoutineSafetyAuditsIncidentReportingStatus.Closed;

        AddDomainEvent(new RoutineSafetyAuditClosedEvent(
            RoutineSafetyAuditsIncidentReportingEventId.New(), RoutineSafetyAuditsIncidentReportingId, nextDueAtUtc, closedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IRoutineSafetyAuditsIncidentReportingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params RoutineSafetyAuditsIncidentReportingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for RoutineSafetyAuditsIncidentReporting {RoutineSafetyAuditsIncidentReportingId}.");
    }
}
