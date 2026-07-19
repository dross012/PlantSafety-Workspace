// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Domain
//  File:           RegulatoryCalendar.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  regulatory-calendar record shape. NOTE: no dedicated field-level
//                  spec was found in project_plantsafety_domains.md (v4.0 20-module
//                  list); project_plantsafety_overview.md Section 6 confirms
//                  "Regulatory Calendar" (4.48) as a distinct sub-module. Properties
//                  below use industry-standard compliance-calendar fields (regulatory
//                  body, jurisdiction, due date, reminder lead time) for this entity
//                  type. Flagged as partially-unsourced.
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the RegulatoryCalendar bounded context. Tracks a
//                  single regulatory obligation/event: its governing body,
//                  jurisdiction, due date, and reminder lead time.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RegulatoryCalendar.Aggregates;

/// <summary>Aggregate root for the RegulatoryCalendar bounded context.</summary>
public sealed partial class RegulatoryCalendar
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this RegulatoryCalendar aggregate.</summary>
    public RegulatoryCalendarId RegulatoryCalendarId { get; private set; }

    /// <summary>Tenant that owns this regulatory-calendar record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this obligation is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public RegulatoryCalendarStatus Status { get; private set; }

    // ---- Regulatory-calendar-specific business properties --------------------

    /// <summary>Human-readable title of the regulatory event/obligation.</summary>
    public string EventTitle { get; private set; } = string.Empty;

    /// <summary>Governing regulatory body (e.g., "OSHA", "EPA", "State DOL").</summary>
    public string RegulatoryBody { get; private set; } = string.Empty;

    /// <summary>Jurisdiction code the obligation applies to (e.g., "US-CA", "US-FED").</summary>
    public string JurisdictionCode { get; private set; } = string.Empty;

    /// <summary>Compliance category (e.g., Reporting, Inspection, Renewal, Filing).</summary>
    public string ComplianceCategory { get; private set; } = string.Empty;

    /// <summary>Identity of the party accountable for meeting this obligation.</summary>
    public UserId ResponsiblePartyId { get; private set; }

    /// <summary>UTC timestamp the obligation is due.</summary>
    public UtcTimestamp? DueAtUtc { get; private set; }

    /// <summary>Number of days before <see cref="DueAtUtc"/> that reminders should fire.</summary>
    public int ReminderLeadDays { get; private set; } = 30;

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IRegulatoryCalendarDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IRegulatoryCalendarDomainEvent> DomainEvents
        => new ReadOnlyCollection<IRegulatoryCalendarDomainEvent>(_domainEvents);

    // For rehydration only.
    private RegulatoryCalendar() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new RegulatoryCalendar aggregate and raises <see cref="RegulatoryCalendarCreatedEvent"/>.</summary>
    public static RegulatoryCalendar Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string eventTitle = "",
        string regulatoryBody = "",
        string jurisdictionCode = "",
        string complianceCategory = "",
        UserId responsiblePartyId = default,
        UtcTimestamp? dueAtUtc = null,
        int reminderLeadDays = 30)
    {
        var agg = new RegulatoryCalendar
        {
            RegulatoryCalendarId  = RegulatoryCalendarId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = RegulatoryCalendarStatus.Created,
            EventTitle            = (eventTitle ?? string.Empty).Trim(),
            RegulatoryBody        = (regulatoryBody ?? string.Empty).Trim(),
            JurisdictionCode      = (jurisdictionCode ?? string.Empty).Trim(),
            ComplianceCategory    = (complianceCategory ?? string.Empty).Trim(),
            ResponsiblePartyId    = responsiblePartyId,
            DueAtUtc              = dueAtUtc,
            ReminderLeadDays      = reminderLeadDays < 0 ? 0 : reminderLeadDays,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new RegulatoryCalendarCreatedEvent(
            EventId:       new RegulatoryCalendarEventId(Uuid7.New()),
            RegulatoryCalendarId:      agg.RegulatoryCalendarId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IRegulatoryCalendarDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params RegulatoryCalendarStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for RegulatoryCalendar {RegulatoryCalendarId}.");
    }
}
