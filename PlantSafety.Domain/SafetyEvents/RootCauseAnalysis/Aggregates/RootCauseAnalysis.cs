// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Domain
//  File:           RootCauseAnalysis.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the SafetyEvents.RootCauseAnalysis bounded context. Tracks
//                  a single root cause analysis engagement from its initial start through
//                  submission for review, approval/completion, closure, and archival. Phase 4 full
//                  build-out: this bounded context previously had no aggregate at all -- only a
//                  placeholder marker class (RootCauseAnalysisModuleMarker) and a read-only query
//                  slice over the "RootCauseAnalysis" table existed. Field shape (RelatedEventId,
//                  Methodology, Status, CompletedAtUtc, LeadAnalyst) is grounded in that
//                  pre-existing read model's actual columns, not invented from scratch. Mirrors
//                  PlantSafety.Domain.Compliance.Permits.Aggregates.Permits's guarded-transition/
//                  domain-event/exception conventions.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Enums;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Events;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Exceptions;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Rules;
using PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyEvents.RootCauseAnalysis.Aggregates;

/// <summary>Aggregate root for the SafetyEvents.RootCauseAnalysis bounded context.</summary>
public sealed class RootCauseAnalysis
{
    // ---- Identity ------------------------------------------------------------

    /// <summary>Strong-typed identity of this RootCauseAnalysis aggregate.</summary>
    public RootCauseAnalysisId RootCauseAnalysisId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public RootCauseAnalysisStatus Status { get; private set; }

    // ---- Analysis-specific business properties ---------------------------------

    /// <summary>Identifier of the related incident, near-miss, or other safety event, if any.</summary>
    public Guid? RelatedEventId { get; private set; }

    /// <summary>Analysis methodology used (e.g., FiveWhys/FishboneDiagram/FaultTree/Bowtie).</summary>
    public string Methodology { get; private set; } = string.Empty;

    /// <summary>Identity of the lead analyst.</summary>
    public UserId LeadAnalyst { get; private set; }

    // ---- Lifecycle attributes (SubmitForReview/Approve/Close/Archive) ---------

    /// <summary>Identity of the user who submitted the analysis for review.</summary>
    public UserId? SubmittedBy { get; private set; }

    /// <summary>UTC instant the analysis was submitted for review.</summary>
    public UtcTimestamp? SubmittedAtUtc { get; private set; }

    /// <summary>Identity of the user who approved the analysis.</summary>
    public UserId? ApprovedBy { get; private set; }

    /// <summary>UTC instant the analysis was reviewed and approved (marked completed).</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>Identity of the user who formally closed the analysis.</summary>
    public UserId? ClosedBy { get; private set; }

    /// <summary>UTC instant the analysis was closed.</summary>
    public UtcTimestamp? ClosedAtUtc { get; private set; }

    /// <summary>UTC instant the analysis was archived for compliance retention.</summary>
    public UtcTimestamp? ArchivedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user/system that created this record.</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events ---------------------------------------------------------

    private readonly List<IRootCauseAnalysisDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IRootCauseAnalysisDomainEvent> DomainEvents
        => new ReadOnlyCollection<IRootCauseAnalysisDomainEvent>(_domainEvents);

    // For rehydration only.
    private RootCauseAnalysis() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Starts a new RootCauseAnalysis and records <see cref="RootCauseAnalysisStartedEvent"/>.</summary>
    public static RootCauseAnalysis Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        Guid? relatedEventId,
        string methodology,
        UserId leadAnalyst)
    {
        if (string.IsNullOrWhiteSpace(methodology))
            throw new ArgumentException("Methodology must be supplied.", nameof(methodology));

        var agg = new RootCauseAnalysis
        {
            RootCauseAnalysisId = RootCauseAnalysisId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = RootCauseAnalysisStatus.InProgress,
            RelatedEventId = relatedEventId,
            Methodology = methodology.Trim(),
            LeadAnalyst = leadAnalyst,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new RootCauseAnalysisStartedEvent(
            EventId: RootCauseAnalysisEventId.New(),
            RootCauseAnalysisId: agg.RootCauseAnalysisId,
            TenantId: tenantId,
            SiteId: siteId,
            RelatedEventId: relatedEventId,
            Methodology: agg.Methodology,
            LeadAnalyst: leadAnalyst,
            StartedAtUtc: createdAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));

        return agg;
    }

    // ---- Lifecycle: SubmitForReview -------------------------------------------

    /// <summary>
    /// Submits the analysis for review (InProgress -&gt; PendingReview). Valid only from
    /// <see cref="RootCauseAnalysisStatus.InProgress"/>.
    /// </summary>
    public void SubmitForReview(
        UserId submittedBy,
        UtcTimestamp submittedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RootCauseAnalysisLifecycleRules.CanSubmitForReview, RootCauseAnalysisStatus.PendingReview);

        SubmittedBy = submittedBy;
        SubmittedAtUtc = submittedAtUtc;
        Status = RootCauseAnalysisStatus.PendingReview;

        AddDomainEvent(new RootCauseAnalysisSubmittedForReviewEvent(
            EventId: RootCauseAnalysisEventId.New(),
            RootCauseAnalysisId: RootCauseAnalysisId,
            SubmittedBy: submittedBy,
            SubmittedAtUtc: submittedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Approve -----------------------------------------------------

    /// <summary>
    /// Reviews and approves the analysis (PendingReview -&gt; Completed). Valid only from
    /// <see cref="RootCauseAnalysisStatus.PendingReview"/>.
    /// </summary>
    public void Approve(
        UserId approvedBy,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RootCauseAnalysisLifecycleRules.CanApprove, RootCauseAnalysisStatus.Completed);

        ApprovedBy = approvedBy;
        CompletedAtUtc = completedAtUtc;
        Status = RootCauseAnalysisStatus.Completed;

        AddDomainEvent(new RootCauseAnalysisApprovedEvent(
            EventId: RootCauseAnalysisEventId.New(),
            RootCauseAnalysisId: RootCauseAnalysisId,
            ApprovedBy: approvedBy,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Close ------------------------------------------------------

    /// <summary>
    /// Formally closes a completed analysis (Completed -&gt; Closed). Valid only from
    /// <see cref="RootCauseAnalysisStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RootCauseAnalysisLifecycleRules.CanClose, RootCauseAnalysisStatus.Closed);

        ClosedBy = closedBy;
        ClosedAtUtc = closedAtUtc;
        Status = RootCauseAnalysisStatus.Closed;

        AddDomainEvent(new RootCauseAnalysisClosedEvent(
            EventId: RootCauseAnalysisEventId.New(),
            RootCauseAnalysisId: RootCauseAnalysisId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Archive -----------------------------------------------------

    /// <summary>
    /// Archives a closed analysis for compliance retention (Closed -&gt; Archived). Valid only
    /// from <see cref="RootCauseAnalysisStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, RootCauseAnalysisLifecycleRules.CanArchive, RootCauseAnalysisStatus.Archived);

        ArchivedAtUtc = archivedAtUtc;
        Status = RootCauseAnalysisStatus.Archived;

        AddDomainEvent(new RootCauseAnalysisArchivedEvent(
            EventId: RootCauseAnalysisEventId.New(),
            RootCauseAnalysisId: RootCauseAnalysisId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IRootCauseAnalysisDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<RootCauseAnalysisStatus, bool> guard, RootCauseAnalysisStatus expected)
    {
        if (guard(Status)) return;

        throw new RootCauseAnalysisInvalidStatusTransitionException(
            RootCauseAnalysisId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
