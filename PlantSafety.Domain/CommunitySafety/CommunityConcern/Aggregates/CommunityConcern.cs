// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         CommunitySafety.CommunityConcern
//  Layer:          Domain
//  File:           CommunityConcern.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the CommunityConcern bounded context (Complaint/Concern
//                  Intake subdomain of CommunitySafety, and the primary aggregate for this
//                  module). Tracks a single community concern/complaint from intake (Log) through
//                  triage, optional escalation (recurring-pattern detection -- see
//                  CommunityConcernService -- elevates local RiskManagement visibility), and
//                  resolution. Optionally references the on-site SafetyEvents incident it relates
//                  to via a weak Guid reference (RelatedIncidentId) rather than a direct type
//                  reference, per the Canonical Aggregate Rule / Zero Cross-Module Leakage rule.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Entities;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Events;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Exceptions;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.Rules;
using PlantSafety.Domain.CommunitySafety.CommunityConcern.ValueObjects.Ids;

namespace PlantSafety.Domain.CommunitySafety.CommunityConcern.Aggregates;

/// <summary>Aggregate root for the CommunityConcern bounded context.</summary>
public sealed partial class CommunityConcern
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this CommunityConcern aggregate.</summary>
    public CommunityConcernId CommunityConcernId { get; private set; }

    /// <summary>Tenant that owns this concern record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site the concern relates to.</summary>
    public Guid SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CommunityConcernStatus Status { get; private set; }

    // ---- Concern-specific business properties ---------------------------------

    /// <summary>Classification of the subject matter of the concern.</summary>
    public CommunityConcernCategory Category { get; private set; }

    /// <summary>Free-text severity classification (Low, Medium, High, Critical).</summary>
    public string Severity { get; private set; } = string.Empty;

    /// <summary>Facility area the concern relates to (coarse-grained, used for recurring-pattern detection).</summary>
    public string FacilityArea { get; private set; } = string.Empty;

    /// <summary>Narrative description of the concern.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Identity of the CommunityContact who reported the concern, if the reporter is a registered contact.</summary>
    public Guid? ReportedByContactId { get; private set; }

    /// <summary>
    /// Weak reference (Guid only, never a direct type reference) to the SafetyEvents incident this
    /// concern relates to, if any. Populated when the concern originates from
    /// CommunityImpactAssessment.RecordCommunityImpactAssessment or is otherwise linked by staff.
    /// </summary>
    public Guid? RelatedIncidentId { get; private set; }

    /// <summary>UTC instant the concern was logged.</summary>
    public UtcTimestamp LoggedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>Number of times this concern has been escalated.</summary>
    public int EscalationCount { get; private set; }

    /// <summary>Reason recorded for the most recent escalation, if any.</summary>
    public string? EscalationReason { get; private set; }

    /// <summary>UTC instant of the most recent escalation, if any.</summary>
    public UtcTimestamp? EscalatedAtUtc { get; private set; }

    /// <summary>Resolution notes recorded when the concern was resolved, if any.</summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>UTC instant the concern was resolved, if any. Drives the concern-resolution-time KPI.</summary>
    public UtcTimestamp? ResolvedAtUtc { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<ICommunityConcernDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICommunityConcernDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICommunityConcernDomainEvent>(_domainEvents);

    // ---- History Entries --------------------------------------------------------

    private readonly List<CommunityConcernHistoryEntry> _pendingHistoryEntries = [];

    /// <summary>History entries appended during this unit of work.</summary>
    public IReadOnlyCollection<CommunityConcernHistoryEntry> PendingHistoryEntries
        => new ReadOnlyCollection<CommunityConcernHistoryEntry>(_pendingHistoryEntries);

    // For rehydration only.
    private CommunityConcern() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>
    /// Logs a new CommunityConcern aggregate and raises <see cref="CommunityConcernLoggedEvent"/>.
    /// </summary>
    /// <exception cref="ArgumentException">A required field is missing or empty.</exception>
    public static CommunityConcern Log(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid siteId,
        CommunityConcernCategory category,
        string severity,
        string facilityArea,
        string description,
        Guid? reportedByContactId = null,
        Guid? relatedIncidentId = null)
    {
        var normalizedSeverity = (severity ?? string.Empty).Trim();
        var normalizedFacilityArea = (facilityArea ?? string.Empty).Trim();
        var normalizedDescription = (description ?? string.Empty).Trim();

        if (tenantId == Guid.Empty)
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (siteId == Guid.Empty)
            throw new ArgumentException("SiteId is required.", nameof(siteId));
        if (string.IsNullOrWhiteSpace(normalizedSeverity))
            throw new ArgumentException("Severity is required.", nameof(severity));
        if (string.IsNullOrWhiteSpace(normalizedFacilityArea))
            throw new ArgumentException("FacilityArea is required.", nameof(facilityArea));
        if (string.IsNullOrWhiteSpace(normalizedDescription))
            throw new ArgumentException("Description is required.", nameof(description));

        var agg = new CommunityConcern
        {
            CommunityConcernId = CommunityConcernId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = CommunityConcernStatus.Open,
            Category = category,
            Severity = normalizedSeverity,
            FacilityArea = normalizedFacilityArea,
            Description = normalizedDescription,
            ReportedByContactId = reportedByContactId,
            RelatedIncidentId = relatedIncidentId,
            LoggedAtUtc = createdAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new CommunityConcernLoggedEvent(
            CommunityConcernEventId.New(), agg.CommunityConcernId, agg.Category, agg.Severity,
            createdAtUtc, actor, correlationId, causationId));

        agg.AddHistoryEntry(createdAtUtc, actor.ToString(), "Logged", $"Concern logged: {normalizedDescription}");

        return agg;
    }

    /// <summary>Reconstructs a CommunityConcern aggregate from previously persisted state, without raising domain events.</summary>
    public static CommunityConcern Rehydrate(
        CommunityConcernId communityConcernId,
        Guid tenantId,
        Guid siteId,
        CommunityConcernStatus status,
        CommunityConcernCategory category,
        string severity,
        string facilityArea,
        string description,
        Guid? reportedByContactId,
        Guid? relatedIncidentId,
        UtcTimestamp loggedAtUtc,
        UtcTimestamp createdAtUtc,
        UserId createdBy,
        int escalationCount,
        string? escalationReason,
        UtcTimestamp? escalatedAtUtc,
        string? resolutionNotes,
        UtcTimestamp? resolvedAtUtc)
        => new()
        {
            CommunityConcernId = communityConcernId,
            TenantId = tenantId,
            SiteId = siteId,
            Status = status,
            Category = category,
            Severity = severity,
            FacilityArea = facilityArea,
            Description = description,
            ReportedByContactId = reportedByContactId,
            RelatedIncidentId = relatedIncidentId,
            LoggedAtUtc = loggedAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
            EscalationCount = escalationCount,
            EscalationReason = escalationReason,
            EscalatedAtUtc = escalatedAtUtc,
            ResolutionNotes = resolutionNotes,
            ResolvedAtUtc = resolvedAtUtc,
        };

    // ---- Lifecycle: BeginReview ------------------------------------------------

    /// <summary>Transitions the concern into active review/investigation.</summary>
    /// <exception cref="CommunityConcernInvalidStatusTransitionException">The transition is not permitted.</exception>
    public void BeginReview(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CommunityConcernLifecycleRules.CanTransition(Status, CommunityConcernStatus.UnderReview))
            throw new CommunityConcernInvalidStatusTransitionException(
                CommunityConcernId, Status.ToString(), CommunityConcernStatus.UnderReview.ToString(), correlationId, causationId);

        Status = CommunityConcernStatus.UnderReview;
        AddHistoryEntry(nowUtc, actor.ToString(), "StatusChange", "Concern moved to review.");
    }

    // ---- Lifecycle: Escalate ---------------------------------------------------

    /// <summary>
    /// Escalates the concern (e.g. detected as part of a recurring-concern pattern for
    /// <see cref="FacilityArea"/> by <see cref="Services.CommunityConcernService.IsRecurringPattern"/>).
    /// Bumps severity one tier and raises <see cref="CommunityConcernEscalatedEvent"/>, which is the
    /// CommunitySafety Risk Hook consumed by a downstream RiskManagement subscriber to elevate
    /// local risk visibility for <see cref="FacilityArea"/>.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="reason"/> is empty.</exception>
    /// <exception cref="CommunityConcernInvalidStatusTransitionException">The concern is Resolved or Archived.</exception>
    public void Escalate(
        string reason,
        int recentConcernCountForArea,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!CommunityConcernLifecycleRules.CanTransition(Status, CommunityConcernStatus.Escalated))
            throw new CommunityConcernInvalidStatusTransitionException(
                CommunityConcernId, Status.ToString(), CommunityConcernStatus.Escalated.ToString(), correlationId, causationId);

        var normalizedReason = (reason ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedReason))
            throw new ArgumentException("Reason is required to escalate a concern.", nameof(reason));

        Status = CommunityConcernStatus.Escalated;
        Severity = CommunityConcernLifecycleRules.BumpSeverity(Severity);
        EscalationReason = normalizedReason;
        EscalatedAtUtc = nowUtc;
        EscalationCount += 1;

        AddDomainEvent(new CommunityConcernEscalatedEvent(
            CommunityConcernEventId.New(), CommunityConcernId, FacilityArea, normalizedReason,
            recentConcernCountForArea, nowUtc, actor, correlationId, causationId));

        AddHistoryEntry(nowUtc, actor.ToString(), "Escalated", $"Escalated: {normalizedReason}");
    }

    // ---- Lifecycle: Resolve ---------------------------------------------------

    /// <summary>
    /// Resolves the concern. Raises <see cref="CommunityConcernResolvedEvent"/>, whose
    /// <c>ResolutionDuration</c> feeds the concern-resolution-time KPI.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="resolutionNotes"/> is empty.</exception>
    /// <exception cref="CommunityConcernInvalidStatusTransitionException">The concern is already Resolved or Archived.</exception>
    public void Resolve(
        string resolutionNotes,
        UtcTimestamp nowUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        if (!CommunityConcernLifecycleRules.CanTransition(Status, CommunityConcernStatus.Resolved))
            throw new CommunityConcernInvalidStatusTransitionException(
                CommunityConcernId, Status.ToString(), CommunityConcernStatus.Resolved.ToString(), correlationId, causationId);

        var normalizedNotes = (resolutionNotes ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedNotes))
            throw new ArgumentException("ResolutionNotes is required to resolve a concern.", nameof(resolutionNotes));

        Status = CommunityConcernStatus.Resolved;
        ResolutionNotes = normalizedNotes;
        ResolvedAtUtc = nowUtc;

        AddDomainEvent(new CommunityConcernResolvedEvent(
            CommunityConcernEventId.New(), CommunityConcernId, normalizedNotes, LoggedAtUtc, nowUtc, actor, correlationId, causationId));

        AddHistoryEntry(nowUtc, actor.ToString(), "Resolution", normalizedNotes);
    }

    // ---- Lifecycle: Archive ---------------------------------------------------

    /// <summary>Archives (soft-deletes) the resolved concern once the data-governance retention window has elapsed.</summary>
    /// <exception cref="CommunityConcernInvalidStatusTransitionException">The transition is not permitted.</exception>
    public void Archive(UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!CommunityConcernLifecycleRules.CanTransition(Status, CommunityConcernStatus.Archived))
            throw new CommunityConcernInvalidStatusTransitionException(
                CommunityConcernId, Status.ToString(), CommunityConcernStatus.Archived.ToString(), correlationId, causationId);

        Status = CommunityConcernStatus.Archived;
        AddHistoryEntry(nowUtc, actor.ToString(), "StatusChange", "Concern archived per data-governance retention policy.");
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICommunityConcernDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddHistoryEntry(UtcTimestamp timestampUtc, string actor, string eventType, string description)
    {
        _pendingHistoryEntries.Add(new CommunityConcernHistoryEntry(
            entryId: Guid.NewGuid(),
            communityConcernId: CommunityConcernId,
            timestampUtc: timestampUtc,
            actor: actor,
            eventType: eventType,
            description: description));
    }

    /// <summary>Clears the in-memory pending history entry list after persistence.</summary>
    public void ClearPendingHistoryEntries() => _pendingHistoryEntries.Clear();
}
