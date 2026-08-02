// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.NearMisses
//  Layer:          Domain
//  File:           NearMisses.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the SafetyEvents.NearMisses bounded context. Tracks a
//                  single near-miss report from its initial capture through review, completion,
//                  closure, and archival. Phase 4 full build-out: this bounded context previously
//                  had no aggregate at all -- only a placeholder marker class
//                  (NearMissesModuleMarker) and a read-only query slice over the "NearMisses"
//                  table existed. Field shape (Category, PotentialSeverity, Status, ReportedAtUtc,
//                  ReportedBy) is grounded in that pre-existing read model's actual columns, not
//                  invented from scratch. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Aggregates.Permits's guarded-transition/
//                  domain-event/exception conventions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.NearMisses.Enums;
using PlantSafety.Domain.SafetyEvents.NearMisses.Events;
using PlantSafety.Domain.SafetyEvents.NearMisses.Exceptions;
using PlantSafety.Domain.SafetyEvents.NearMisses.Rules;
using PlantSafety.Domain.SafetyEvents.NearMisses.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyEvents.NearMisses.Aggregates;

/// <summary>Aggregate root for the SafetyEvents.NearMisses bounded context.</summary>
public sealed class NearMisses
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this NearMisses aggregate.</summary>
    public NearMissesId NearMissesId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public NearMissesStatus Status { get; private set; }

    // ---- Near-miss-specific business properties -------------------------------

    /// <summary>Hazard category (e.g., Slip/Trip/Fall, EquipmentFailure, ChemicalExposure).</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Potential severity had the event resulted in an actual incident (e.g., Low/Medium/High/Critical).</summary>
    public string PotentialSeverity { get; private set; } = string.Empty;

    /// <summary>UTC instant the near-miss was reported.</summary>
    public UtcTimestamp ReportedAtUtc { get; private set; }

    // ---- Lifecycle attributes (StartReview/Complete/Close/Archive) ------------

    /// <summary>Identity of the reviewer who started reviewing this near-miss.</summary>
    public UserId? ReviewStartedBy { get; private set; }

    /// <summary>UTC instant the review started.</summary>
    public UtcTimestamp? ReviewStartedAtUtc { get; private set; }

    /// <summary>Identity of the user who completed the review.</summary>
    public UserId? CompletedBy { get; private set; }

    /// <summary>Free-text findings recorded when the review is completed.</summary>
    public string? Findings { get; private set; }

    /// <summary>UTC instant the review was completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Identity of the user who formally closed this near-miss.</summary>
    public UserId? ClosedBy { get; private set; }

    /// <summary>UTC instant the near-miss was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC instant the near-miss was archived for compliance retention.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<INearMissesDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<INearMissesDomainEvent> DomainEvents
        => new ReadOnlyCollection<INearMissesDomainEvent>(_domainEvents);

    // For rehydration only.
    private NearMisses() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Reports a new NearMisses record and records <see cref="NearMissesReportedEvent"/>.</summary>
    public static NearMisses Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string category,
        string potentialSeverity,
        UtcTimestamp reportedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category must be supplied.", nameof(category));
        if (string.IsNullOrWhiteSpace(potentialSeverity))
            throw new ArgumentException("PotentialSeverity must be supplied.", nameof(potentialSeverity));

        var agg = new NearMisses
        {
            NearMissesId = NearMissesId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = NearMissesStatus.Reported,
            Category = category.Trim(),
            PotentialSeverity = potentialSeverity.Trim(),
            ReportedAtUtc = reportedAtUtc,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new NearMissesReportedEvent(
            EventId: NearMissesEventId.New(),
            NearMissesId: agg.NearMissesId,
            TenantId: tenantId,
            SiteId: siteId,
            Category: agg.Category,
            PotentialSeverity: agg.PotentialSeverity,
            ReportedAtUtc: reportedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle: StartReview ----------------------------------------------

    /// <summary>
    /// Starts a reviewer's assessment of the near-miss (Reported -&gt; UnderReview). Valid only
    /// from <see cref="NearMissesStatus.Reported"/>.
    /// </summary>
    public void StartReview(
        UserId reviewedBy,
        UtcTimestamp reviewStartedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, NearMissesLifecycleRules.CanStartReview, NearMissesStatus.UnderReview);

        ReviewStartedBy = reviewedBy;
        ReviewStartedAtUtc = reviewStartedAtUtc;
        Status = NearMissesStatus.UnderReview;

        AddDomainEvent(new NearMissesReviewStartedEvent(
            EventId: NearMissesEventId.New(),
            NearMissesId: NearMissesId,
            ReviewStartedBy: reviewedBy,
            ReviewStartedAtUtc: reviewStartedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Complete --------------------------------------------------

    /// <summary>
    /// Completes the review and records findings (UnderReview -&gt; Completed). Valid only from
    /// <see cref="NearMissesStatus.UnderReview"/>.
    /// </summary>
    public void Complete(
        UserId completedBy,
        string? findings,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, NearMissesLifecycleRules.CanComplete, NearMissesStatus.Completed);

        CompletedBy = completedBy;
        Findings = string.IsNullOrWhiteSpace(findings) ? null : findings.Trim();
        CompletedAtUtc = completedAtUtc;
        Status = NearMissesStatus.Completed;

        AddDomainEvent(new NearMissesCompletedEvent(
            EventId: NearMissesEventId.New(),
            NearMissesId: NearMissesId,
            CompletedBy: completedBy,
            Findings: Findings,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close ------------------------------------------------------

    /// <summary>
    /// Formally closes a completed near-miss (Completed -&gt; Closed). Valid only from
    /// <see cref="NearMissesStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, NearMissesLifecycleRules.CanClose, NearMissesStatus.Closed);

        ClosedBy = closedBy;
        ClosedAtUtc = closedAtUtc;
        Status = NearMissesStatus.Closed;

        AddDomainEvent(new NearMissesClosedEvent(
            EventId: NearMissesEventId.New(),
            NearMissesId: NearMissesId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Archive -----------------------------------------------------

    /// <summary>
    /// Archives a closed near-miss for compliance retention (Closed -&gt; Archived). Valid only
    /// from <see cref="NearMissesStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, NearMissesLifecycleRules.CanArchive, NearMissesStatus.Archived);

        ArchivedAtUtc = archivedAtUtc;
        Status = NearMissesStatus.Archived;

        AddDomainEvent(new NearMissesArchivedEvent(
            EventId: NearMissesEventId.New(),
            NearMissesId: NearMissesId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(INearMissesDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<NearMissesStatus, bool> guard, NearMissesStatus expected)
    {
        if (guard(Status)) return;

        throw new NearMissesInvalidStatusTransitionException(
            NearMissesId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
