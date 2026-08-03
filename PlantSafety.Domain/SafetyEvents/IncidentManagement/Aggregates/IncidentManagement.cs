// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagement.cs
//  Created:        2026-06-17
//  Updated:        2026-07-27 -- Redesigned from an anemic, fully-disconnected scaffold (a bare
//                  Create() factory plus generic Created/Active/Completed/Closed/Archived states
//                  matched by nothing in the real persistence layer, zero repository
//                  implementation, zero callers anywhere in Application/Infrastructure/Api) into
//                  the real behavioral model of the production [SafetyEvents].[Incident] write
//                  path (usp_Incident_Create / usp_Incident_Update / usp_Incident_SoftDelete via
//                  IIncidentRepository / IncidentDapperRepository). Fields realigned to the actual
//                  table columns (dropped ungrounded InjuryType/RootCause/CorrectiveActions text
//                  fields that had no backing column or procedure anywhere); Status realigned to
//                  the two states the procedures actually produce (Reported, Archived); added
//                  UpdateDetails/Archive lifecycle methods and a Rehydrate factory so validation
//                  that previously lived only in Application-layer FluentValidation can also be
//                  enforced here; every transition now raises a domain event
//                  (Created/Updated/Archived) per the Canonical Aggregate Rule. The dead sibling
//                  scaffold files (Factory, ArchivalPolicy, LifecycleRules, Service placeholder,
//                  Specification base classes, HistoryEntry, Snapshot, and the unimplemented
//                  aggregate-shaped IIncidentManagementRepository) were removed as part of this
//                  pass -- see docs/architecture/plantsafety-architecture.md for the full
//                  before/after inventory. Additive-only evolution.
//  Author:         David (Ted) + Copilot / PlantSafety Platform Team (Claude Code refactoring-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the IncidentManagement bounded context. Tracks a single
//                  safety incident: its title, description, severity, location, reporter and
//                  assigned investigator, and archival. Onboarding note: the production
//                  Create/Update/SoftDelete MediatR handlers under
//                  PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement
//                  currently call IIncidentRepository directly with primitives (a deliberate,
//                  existing performance choice in this codebase -- partial updates via SQL
//                  ISNULL/COALESCE rather than full aggregate load/save). This aggregate is the
//                  authoritative, testable model of the same invariants and is intended to be the
//                  target for any future handler wired through full aggregate load/save.
//                  Additive-only evolution.
//  Updated:        2026-08-03 -- Added the AssignTechnician/Escalate/AddNote/Resolve lifecycle
//                  commands (UI Gaps Scoping, IncidentManagement, Part 1) plus a pending
//                  IncidentManagementTimelineEntry list mirroring the existing DomainEvents
//                  in-memory pattern, so GetIncidentTimelineQuery can be served from real
//                  persisted rows rather than reconstructed from the outbox. Additive-only
//                  evolution.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.IncidentManagement.Entities;

namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.Aggregates;

/// <summary>Aggregate root for the IncidentManagement bounded context.</summary>
public sealed partial class IncidentManagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this IncidentManagement aggregate.</summary>
    public IncidentManagementId IncidentManagementId { get; private set; }

    /// <summary>Tenant that owns this incident record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the incident occurred at. Required — mirrors [SafetyEvents].[Incident].SiteId (NOT NULL).</summary>
    public Guid SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public IncidentManagementStatus Status { get; private set; }

    // ---- Incident-specific business properties -----------------------------------

    /// <summary>Human-readable incident title/summary.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Narrative description of what happened.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Incident severity classification (free-text, mirrors Incident.Severity NVARCHAR(30)).</summary>
    public string Severity { get; private set; } = string.Empty;

    /// <summary>Physical location/equipment where the incident occurred.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the incident actually occurred (may differ from ReportedAtUtc).</summary>
    public UtcTimestamp IncidentOccurredAtUtc { get; private set; }

    /// <summary>UTC timestamp the incident was reported (set once at Create; mirrors Incident.ReportedAtUtc).</summary>
    public UtcTimestamp ReportedAtUtc { get; private set; }

    /// <summary>Identity of the worker who reported the incident.</summary>
    public UserId ReportedByWorkerId { get; private set; }

    /// <summary>Identity of the investigator assigned to this incident, if assigned.</summary>
    public UserId? AssignedInvestigatorId { get; private set; }

    /// <summary>UTC instant this record was archived (soft-deleted), once archived.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>Reason recorded for archival, once archived.</summary>
    public string? DeletionReason { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Lifecycle: Assign/Escalate/Resolve business properties -------------

    /// <summary>Reason recorded for the most recent escalation, if any.</summary>
    public string? EscalationReason { get; private set; }

    /// <summary>UTC instant of the most recent escalation, if any.</summary>
    public UtcTimestamp? EscalatedAtUtc { get; private set; }

    /// <summary>Number of times this incident has been escalated.</summary>
    public int EscalationCount { get; private set; }

    /// <summary>Resolution notes recorded when the incident was resolved, if any.</summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>UTC instant the incident was resolved, if any. Persisted to Incident.ClosedAtUtc.</summary>
    public UtcTimestamp? ResolvedAtUtc { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IIncidentManagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IIncidentManagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<IIncidentManagementDomainEvent>(_domainEvents);

    // ---- Timeline Entries ----------------------------------------------------

    private readonly List<IncidentManagementTimelineEntry> _pendingTimelineEntries = [];

    /// <summary>
    /// Timeline entries appended during this unit of work (mirrors <see cref="DomainEvents"/>
    /// in-memory-then-clear pattern). Persisted by the Infrastructure repository after commit.
    /// </summary>
    public IReadOnlyCollection<IncidentManagementTimelineEntry> PendingTimelineEntries
        => new ReadOnlyCollection<IncidentManagementTimelineEntry>(_pendingTimelineEntries);

    // For rehydration only.
    private IncidentManagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>
    /// Creates a new IncidentManagement aggregate and raises <see cref="IncidentManagementCreatedEvent"/>.
    /// Enforces the required-field invariants implied by the NOT NULL columns on
    /// [SafetyEvents].[Incident] (Title, Description, Severity, Location, SiteId, ReportedByWorkerId).
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static IncidentManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid siteId,
        string title,
        string description,
        string severity,
        string location,
        UserId reportedByWorkerId,
        UtcTimestamp? incidentOccurredAtUtc = null,
        UserId? assignedInvestigatorId = null)
    {
        var normalizedTitle = (title ?? string.Empty).Trim();
        var normalizedDescription = (description ?? string.Empty).Trim();
        var normalizedSeverity = (severity ?? string.Empty).Trim();
        var normalizedLocation = (location ?? string.Empty).Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (siteId == Guid.Empty)
            throw new ArgumentException("SiteId is required.", nameof(siteId));
        if (string.IsNullOrWhiteSpace(normalizedTitle))
            throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(normalizedDescription))
            throw new ArgumentException("Description is required.", nameof(description));
        if (string.IsNullOrWhiteSpace(normalizedSeverity))
            throw new ArgumentException("Severity is required.", nameof(severity));
        if (string.IsNullOrWhiteSpace(normalizedLocation))
            throw new ArgumentException("Location is required.", nameof(location));
        if (reportedByWorkerId == default)
            throw new ArgumentException("ReportedByWorkerId is required.", nameof(reportedByWorkerId));

        var agg = new IncidentManagement
        {
            IncidentManagementId   = IncidentManagementId.New(),
            TenantId                = tenantId,
            SiteId                  = siteId,
            Status                  = IncidentManagementStatus.Reported,
            Title                   = normalizedTitle,
            Description             = normalizedDescription,
            Severity                = normalizedSeverity,
            Location                = normalizedLocation,
            IncidentOccurredAtUtc   = incidentOccurredAtUtc ?? createdAtUtc,
            ReportedAtUtc           = createdAtUtc,
            ReportedByWorkerId      = reportedByWorkerId,
            AssignedInvestigatorId  = assignedInvestigatorId,
            CreatedAtUtc            = createdAtUtc,
            CreatedBy               = createdBy,
        };

        agg.AddDomainEvent(new IncidentManagementCreatedEvent(
            EventId:       IncidentManagementEventId.New(),
            IncidentManagementId: agg.IncidentManagementId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        agg.AddTimelineEntry(
            timestampUtc: createdAtUtc,
            actor: actor.ToString(),
            eventType: "StatusChange",
            severity: agg.Severity,
            description: $"Incident reported: {agg.Title}");

        return agg;
    }

    /// <summary>
    /// Reconstructs an IncidentManagement aggregate from previously persisted state, without
    /// raising any domain events. For use by callers that load current state (e.g. from the
    /// read-side repository) before invoking <see cref="UpdateDetails"/> or <see cref="Archive"/>.
    /// </summary>
    public static IncidentManagement Rehydrate(
        IncidentManagementId incidentManagementId,
        Guid tenantId,
        Guid siteId,
        IncidentManagementStatus status,
        string title,
        string description,
        string severity,
        string location,
        UtcTimestamp incidentOccurredAtUtc,
        UtcTimestamp reportedAtUtc,
        UserId reportedByWorkerId,
        UserId? assignedInvestigatorId,
        UtcTimestamp? archivedAtUtc,
        string? deletionReason,
        UtcTimestamp createdAtUtc,
        UserId createdBy,
        string? escalationReason = null,
        UtcTimestamp? escalatedAtUtc = null,
        int escalationCount = 0,
        string? resolutionNotes = null,
        UtcTimestamp? resolvedAtUtc = null)
        => new()
        {
            IncidentManagementId   = incidentManagementId,
            TenantId                = tenantId,
            SiteId                  = siteId,
            Status                  = status,
            Title                   = title,
            Description             = description,
            Severity                = severity,
            Location                = location,
            IncidentOccurredAtUtc   = incidentOccurredAtUtc,
            ReportedAtUtc           = reportedAtUtc,
            ReportedByWorkerId      = reportedByWorkerId,
            AssignedInvestigatorId  = assignedInvestigatorId,
            ArchivedAtUtc           = archivedAtUtc,
            DeletionReason          = deletionReason,
            CreatedAtUtc            = createdAtUtc,
            CreatedBy               = createdBy,
            EscalationReason        = escalationReason,
            EscalatedAtUtc          = escalatedAtUtc,
            EscalationCount         = escalationCount,
            ResolutionNotes         = resolutionNotes,
            ResolvedAtUtc           = resolvedAtUtc,
        };

    // ---- Lifecycle: Update ---------------------------------------------------

    /// <summary>
    /// Updates mutable incident details. Mirrors the field set and null-means-unchanged semantics
    /// of [SafetyEvents].[usp_Incident_Update] (a <see langword="null"/> argument leaves the
    /// existing value untouched). Raises <see cref="IncidentManagementUpdatedEvent"/>.
    /// </summary>
    /// <exception cref="IncidentManagementInvalidStatusTransitionException">The incident is Archived (terminal state).</exception>
    public void UpdateDetails(
        string? title,
        string? description,
        string? severity,
        string? location,
        UserId? assignedInvestigatorId,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (Status == IncidentManagementStatus.Archived)
            throw new IncidentManagementInvalidStatusTransitionException(
                IncidentManagementId, Status.ToString(), "Updated", correlationId, causationId);

        if (!string.IsNullOrWhiteSpace(title))
            Title = title.Trim();
        if (!string.IsNullOrWhiteSpace(description))
            Description = description.Trim();
        if (!string.IsNullOrWhiteSpace(severity))
            Severity = severity.Trim();
        if (!string.IsNullOrWhiteSpace(location))
            Location = location.Trim();
        if (assignedInvestigatorId is not null)
            AssignedInvestigatorId = assignedInvestigatorId;

        AddDomainEvent(new IncidentManagementUpdatedEvent(
            IncidentManagementEventId.New(), IncidentManagementId, nowUtc, actor, correlationId, causationId));
    }

    // ---- Lifecycle: Assign ---------------------------------------------------

    /// <summary>
    /// (Re)assigns a technician/investigator to the incident. If the incident is still in its
    /// initial <see cref="IncidentManagementStatus.Reported"/> state, transitions it to
    /// <see cref="IncidentManagementStatus.Assigned"/>; reassignment on an already-progressed
    /// incident leaves its current status untouched. Raises
    /// <see cref="IncidentManagementAssignedEvent"/>.
    /// </summary>
    /// <exception cref="IncidentManagementInvalidStatusTransitionException">The incident is Archived (terminal state).</exception>
    public void AssignTechnician(
        UserId technicianId,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (Status == IncidentManagementStatus.Archived)
            throw new IncidentManagementInvalidStatusTransitionException(
                IncidentManagementId, Status.ToString(), "Assigned", correlationId, causationId);

        AssignedInvestigatorId = technicianId;
        if (Status == IncidentManagementStatus.Reported)
            Status = IncidentManagementStatus.Assigned;

        AddDomainEvent(new IncidentManagementAssignedEvent(
            IncidentManagementEventId.New(), IncidentManagementId, technicianId, nowUtc, actor, correlationId, causationId));

        AddTimelineEntry(
            timestampUtc: nowUtc,
            actor: actor.ToString(),
            eventType: "StatusChange",
            severity: Severity,
            description: $"Technician {technicianId.Value} assigned to incident.");
    }

    // ---- Lifecycle: Escalate ---------------------------------------------------

    /// <summary>
    /// Escalates the incident: bumps severity one tier (Minor -&gt; Moderate -&gt; Serious -&gt; Critical,
    /// saturating at Critical), transitions status to <see cref="IncidentManagementStatus.Escalated"/>,
    /// and raises <see cref="IncidentManagementEscalatedEvent"/> so downstream notification consumers
    /// (e.g. supervisor alerting) can react.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="reason"/> is empty.</exception>
    /// <exception cref="IncidentManagementInvalidStatusTransitionException">The incident is Resolved or Archived.</exception>
    public void Escalate(
        string reason,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (Status is IncidentManagementStatus.Archived or IncidentManagementStatus.Resolved)
            throw new IncidentManagementInvalidStatusTransitionException(
                IncidentManagementId, Status.ToString(), "Escalated", correlationId, causationId);

        var normalizedReason = (reason ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedReason))
            throw new ArgumentException("Reason is required to escalate an incident.", nameof(reason));

        var previousSeverity = Severity;
        var newSeverity = BumpSeverity(Severity);

        Status = IncidentManagementStatus.Escalated;
        Severity = newSeverity;
        EscalationReason = normalizedReason;
        EscalatedAtUtc = nowUtc;
        EscalationCount += 1;

        AddDomainEvent(new IncidentManagementEscalatedEvent(
            IncidentManagementEventId.New(), IncidentManagementId, normalizedReason, previousSeverity, newSeverity,
            EscalationCount, nowUtc, actor, correlationId, causationId));

        AddTimelineEntry(
            timestampUtc: nowUtc,
            actor: actor.ToString(),
            eventType: "Escalation",
            severity: newSeverity,
            description: $"Escalated ({previousSeverity} -> {newSeverity}): {normalizedReason}");
    }

    // ---- Lifecycle: AddNote ---------------------------------------------------

    /// <summary>
    /// Appends a technician/operator note to the incident's timeline. Notes do not change
    /// <see cref="Status"/>. Raises <see cref="IncidentManagementNoteAddedEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="text"/> is empty.</exception>
    /// <exception cref="IncidentManagementInvalidStatusTransitionException">The incident is Archived (terminal state).</exception>
    public void AddNote(
        string text,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (Status == IncidentManagementStatus.Archived)
            throw new IncidentManagementInvalidStatusTransitionException(
                IncidentManagementId, Status.ToString(), "NoteAdded", correlationId, causationId);

        var normalizedText = (text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedText))
            throw new ArgumentException("Text is required to add a note.", nameof(text));

        var noteId = Guid.NewGuid();

        AddDomainEvent(new IncidentManagementNoteAddedEvent(
            IncidentManagementEventId.New(), IncidentManagementId, noteId, normalizedText, nowUtc, actor, correlationId, causationId));

        AddTimelineEntry(
            timestampUtc: nowUtc,
            actor: actor.ToString(),
            eventType: "Note",
            severity: Severity,
            description: normalizedText);
    }

    // ---- Lifecycle: Resolve ---------------------------------------------------

    /// <summary>
    /// Resolves the incident: transitions status to <see cref="IncidentManagementStatus.Resolved"/>,
    /// records <paramref name="resolutionNotes"/> and <see cref="ResolvedAtUtc"/> (persisted to the
    /// existing Incident.ClosedAtUtc column). Raises <see cref="IncidentManagementResolvedEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="resolutionNotes"/> is empty.</exception>
    /// <exception cref="IncidentManagementInvalidStatusTransitionException">The incident is already Resolved or Archived.</exception>
    public void Resolve(
        string resolutionNotes,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (Status is IncidentManagementStatus.Archived or IncidentManagementStatus.Resolved)
            throw new IncidentManagementInvalidStatusTransitionException(
                IncidentManagementId, Status.ToString(), "Resolved", correlationId, causationId);

        var normalizedNotes = (resolutionNotes ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedNotes))
            throw new ArgumentException("ResolutionNotes is required to resolve an incident.", nameof(resolutionNotes));

        Status = IncidentManagementStatus.Resolved;
        ResolutionNotes = normalizedNotes;
        ResolvedAtUtc = nowUtc;

        AddDomainEvent(new IncidentManagementResolvedEvent(
            IncidentManagementEventId.New(), IncidentManagementId, normalizedNotes, nowUtc, actor, correlationId, causationId));

        AddTimelineEntry(
            timestampUtc: nowUtc,
            actor: actor.ToString(),
            eventType: "Resolution",
            severity: Severity,
            description: normalizedNotes);
    }

    /// <summary>Bumps a free-text severity string one tier: Minor -&gt; Moderate -&gt; Serious -&gt; Critical (saturating).</summary>
    private static string BumpSeverity(string currentSeverity) => (currentSeverity ?? string.Empty).Trim().ToLowerInvariant() switch
    {
        "minor" => "Moderate",
        "moderate" => "Serious",
        "serious" => "Critical",
        _ => "Critical",
    };

    // ---- Lifecycle: Archive ---------------------------------------------------

    /// <summary>
    /// Archives (soft-deletes) the incident. Mirrors [SafetyEvents].[usp_Incident_SoftDelete]
    /// (Status -&gt; Archived, IsDeleted -&gt; 1, DeletionReason required). Raises
    /// <see cref="IncidentManagementArchivedEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="deletionReason"/> is empty.</exception>
    /// <exception cref="IncidentManagementInvalidStatusTransitionException">The incident is already Archived.</exception>
    public void Archive(
        string deletionReason,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (Status == IncidentManagementStatus.Archived)
            throw new IncidentManagementInvalidStatusTransitionException(
                IncidentManagementId, Status.ToString(), IncidentManagementStatus.Archived.ToString(), correlationId, causationId);

        var normalizedReason = (deletionReason ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedReason))
            throw new ArgumentException("DeletionReason is required to archive an incident.", nameof(deletionReason));

        Status = IncidentManagementStatus.Archived;
        DeletionReason = normalizedReason;
        ArchivedAtUtc = nowUtc;

        AddDomainEvent(new IncidentManagementArchivedEvent(
            IncidentManagementEventId.New(), IncidentManagementId, nowUtc, normalizedReason, actor, correlationId, causationId));

        AddTimelineEntry(
            timestampUtc: nowUtc,
            actor: actor.ToString(),
            eventType: "StatusChange",
            severity: Severity,
            description: $"Archived: {normalizedReason}");
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IIncidentManagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddTimelineEntry(
        UtcTimestamp timestampUtc,
        string actor,
        string eventType,
        string severity,
        string description)
    {
        _pendingTimelineEntries.Add(new IncidentManagementTimelineEntry(
            entryId: Guid.NewGuid(),
            incidentManagementId: IncidentManagementId,
            timestampUtc: timestampUtc,
            actor: actor,
            eventType: eventType,
            severity: severity,
            description: description));
    }

    /// <summary>Clears the in-memory pending timeline entry list after persistence.</summary>
    public void ClearPendingTimelineEntries() => _pendingTimelineEntries.Clear();
}
