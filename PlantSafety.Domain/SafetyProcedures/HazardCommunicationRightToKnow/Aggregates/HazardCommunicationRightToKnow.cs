// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.HazardCommunicationRightToKnow
//  Layer:          Domain
//  File:           HazardCommunicationRightToKnow.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single hazard communication / right-to-know procedure document.
//                  Ensures employees understand chemical/physical hazards via GHS labeling, accessible
//                  SDS, and hazard-handling training documentation. Models the real document lifecycle:
//                  drafting, approval, publication, scheduled revision, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.HazardCommunicationRightToKnow.Aggregates;

/// <summary>Aggregate root for a single hazard communication / right-to-know procedure document.</summary>
public sealed partial class HazardCommunicationRightToKnow
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this HazardCommunicationRightToKnow aggregate.</summary>
    public HazardCommunicationRightToKnowId HazardCommunicationRightToKnowId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public HazardCommunicationRightToKnowStatus Status { get; private set; }

    // ---- Procedure identity ---------------------------------------------------------

    /// <summary>Human-readable title of this procedure document.</summary>
    public string ProcedureTitle { get; private set; } = string.Empty;

    /// <summary>GHS hazard classification.</summary>
    public ProcedureHazardClass HazardClass { get; private set; }

    /// <summary>Reference to the associated Safety Data Sheet(s).</summary>
    public string SdsReference { get; private set; } = string.Empty;

    /// <summary>Identity of the approver.</summary>
    public UserId ApprovedByUserId { get; private set; }

    /// <summary>UTC timestamp the document was published, if any.</summary>
    public UtcTimestamp? PublishedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next scheduled review is due.</summary>
    public UtcTimestamp? NextReviewDueAtUtc { get; private set; }

    /// <summary>Number of chemical/physical items labeled under this procedure.</summary>
    public int LabeledItemCount { get; private set; }

    /// <summary>Compliance score (0-100) from the most recent audit of this procedure's application.</summary>
    public decimal? ComplianceScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IHazardCommunicationRightToKnowDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IHazardCommunicationRightToKnowDomainEvent> DomainEvents
        => new ReadOnlyCollection<IHazardCommunicationRightToKnowDomainEvent>(_domainEvents);

    // For rehydration only.
    private HazardCommunicationRightToKnow() { }

    // ---- Factory: Create (Draft) -----------------------------------------------------------

    /// <summary>Drafts a new hazard communication procedure and raises <see cref="HazComProcedureDraftedEvent"/>.</summary>
    public static HazardCommunicationRightToKnow Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string procedureTitle = "",
        ProcedureHazardClass hazardClass = ProcedureHazardClass.Chemical,
        string sdsReference = "")
    {
        var agg = new HazardCommunicationRightToKnow
        {
            HazardCommunicationRightToKnowId = HazardCommunicationRightToKnowId.New(),
            TenantId       = tenantId,
            SiteId         = siteId,
            Status         = HazardCommunicationRightToKnowStatus.Draft,
            ProcedureTitle = (procedureTitle ?? string.Empty).Trim(),
            HazardClass    = hazardClass,
            SdsReference   = (sdsReference ?? string.Empty).Trim(),
            CreatedAtUtc   = createdAtUtc,
            CreatedBy      = createdBy,
        };

        agg.AddDomainEvent(new HazComProcedureDraftedEvent(
            HazardCommunicationRightToKnowEventId.New(), agg.HazardCommunicationRightToKnowId, hazardClass, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: SubmitForApproval -----------------------------------------------

    /// <summary>Submits the draft for approval.</summary>
    public void SubmitForApproval(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HazardCommunicationRightToKnowStatus.Draft, HazardCommunicationRightToKnowStatus.UnderRevision);

        Status = HazardCommunicationRightToKnowStatus.PendingApproval;

        AddDomainEvent(new HazComProcedureSubmittedForApprovalEvent(
            HazardCommunicationRightToKnowEventId.New(), HazardCommunicationRightToKnowId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Publish -----------------------------------------------------------

    /// <summary>Approves and publishes the procedure document.</summary>
    public void Publish(UserId approvedByUserId, int labeledItemCount, UtcTimestamp publishedAtUtc, int reviewCycleDays, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HazardCommunicationRightToKnowStatus.PendingApproval);

        if (labeledItemCount < 0)
            throw new ArgumentOutOfRangeException(nameof(labeledItemCount), "Labeled item count cannot be negative.");

        ApprovedByUserId    = approvedByUserId;
        LabeledItemCount    = labeledItemCount;
        PublishedAtUtc      = publishedAtUtc;
        NextReviewDueAtUtc  = new UtcTimestamp(publishedAtUtc.Value.AddDays(reviewCycleDays));
        Status              = HazardCommunicationRightToKnowStatus.Published;

        AddDomainEvent(new HazComProcedurePublishedEvent(
            HazardCommunicationRightToKnowEventId.New(), HazardCommunicationRightToKnowId, approvedByUserId, publishedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ScheduleRevision -----------------------------------------------

    /// <summary>Opens a revision cycle for a published procedure.</summary>
    public void ScheduleRevision(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(HazardCommunicationRightToKnowStatus.Published);

        Status = HazardCommunicationRightToKnowStatus.UnderRevision;

        AddDomainEvent(new HazComProcedureRevisionScheduledEvent(
            HazardCommunicationRightToKnowEventId.New(), HazardCommunicationRightToKnowId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires the procedure document.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == HazardCommunicationRightToKnowStatus.Retired)
            throw new HazardCommunicationRightToKnowInvalidStatusTransitionException(HazardCommunicationRightToKnowId, Status.ToString(), HazardCommunicationRightToKnowStatus.Retired.ToString(), correlationId, causationId);

        Status = HazardCommunicationRightToKnowStatus.Retired;

        AddDomainEvent(new HazComProcedureRetiredEvent(
            HazardCommunicationRightToKnowEventId.New(), HazardCommunicationRightToKnowId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IHazardCommunicationRightToKnowDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params HazardCommunicationRightToKnowStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for HazardCommunicationRightToKnow {HazardCommunicationRightToKnowId}.");
    }
}
