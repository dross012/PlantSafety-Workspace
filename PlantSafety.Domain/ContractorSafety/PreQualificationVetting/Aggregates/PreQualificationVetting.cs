// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.PreQualificationVetting
//  Layer:          Domain
//  File:           PreQualificationVetting.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single contractor pre-qualification/vetting record. Screens
//                  contractors before facility entry via safety performance metrics (EMR, TRIR),
//                  certifications, licenses, insurance coverage, and OSHA training compliance. Models
//                  the real lifecycle: submission, review, approval/rejection, and annual expiry.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.PreQualificationVetting.Aggregates;

/// <summary>Aggregate root for a single contractor pre-qualification/vetting record.</summary>
public sealed partial class PreQualificationVetting
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PreQualificationVetting aggregate.</summary>
    public PreQualificationVettingId PreQualificationVettingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PreQualificationVettingStatus Status { get; private set; }

    // ---- Contractor identity -------------------------------------------------

    /// <summary>Human-readable title of this vetting record.</summary>
    public string VettingTitle { get; private set; } = string.Empty;

    /// <summary>Contractor company name being vetted.</summary>
    public string ContractorName { get; private set; } = string.Empty;

    /// <summary>Experience Modification Rate (EMR). Industry average is 1.0; lower is better.</summary>
    public decimal? EmrScore { get; private set; }

    /// <summary>Total Recordable Incident Rate (TRIR).</summary>
    public decimal? TrirScore { get; private set; }

    /// <summary>Whether OSHA training compliance (10-hour/30-hour) has been confirmed.</summary>
    public bool OshaTrainingCompliant { get; private set; }

    /// <summary>Count of verified insurance/certification/licensing deficiencies found during review.</summary>
    public int DeficiencyCount { get; private set; }

    /// <summary>UTC timestamp insurance coverage expires, if recorded.</summary>
    public UtcTimestamp? InsuranceExpiresAtUtc { get; private set; }

    /// <summary>Identity of the reviewer.</summary>
    public UserId ReviewedByUserId { get; private set; }

    /// <summary>UTC timestamp the vetting decision was recorded, if any.</summary>
    public UtcTimestamp? VettedAtUtc { get; private set; }

    /// <summary>UTC timestamp this approval expires and re-vetting is required.</summary>
    public UtcTimestamp? ExpiresAtUtc { get; private set; }

    /// <summary>Narrative reason recorded on rejection.</summary>
    public string? RejectionReason { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPreQualificationVettingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPreQualificationVettingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPreQualificationVettingDomainEvent>(_domainEvents);

    // For rehydration only.
    private PreQualificationVetting() { }

    // ---- Factory: Create (Submit) -----------------------------------------------------------

    /// <summary>Submits a new contractor for pre-qualification review and raises <see cref="PreQualificationVettingSubmittedEvent"/>.</summary>
    public static PreQualificationVetting Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string vettingTitle = "",
        string contractorName = "")
    {
        var agg = new PreQualificationVetting
        {
            PreQualificationVettingId = PreQualificationVettingId.New(),
            TenantId       = tenantId,
            SiteId         = siteId,
            Status         = PreQualificationVettingStatus.SubmittedForReview,
            VettingTitle   = (vettingTitle ?? string.Empty).Trim(),
            ContractorName = (contractorName ?? string.Empty).Trim(),
            CreatedAtUtc   = createdAtUtc,
            CreatedBy      = createdBy,
        };

        agg.AddDomainEvent(new PreQualificationVettingSubmittedEvent(
            PreQualificationVettingEventId.New(), agg.PreQualificationVettingId, agg.ContractorName,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: BeginReview -----------------------------------------------------------

    /// <summary>Begins the vetting review (safety metrics, certifications, insurance, OSHA compliance).</summary>
    public void BeginReview(UserId reviewedByUserId, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PreQualificationVettingStatus.SubmittedForReview, PreQualificationVettingStatus.Expired);

        ReviewedByUserId = reviewedByUserId;
        Status = PreQualificationVettingStatus.UnderReview;

        AddDomainEvent(new PreQualificationVettingReviewStartedEvent(
            PreQualificationVettingEventId.New(), PreQualificationVettingId, reviewedByUserId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Approve -----------------------------------------------------------

    /// <summary>Approves the contractor based on recorded safety metrics, certifications, and insurance.</summary>
    public void Approve(
        decimal emrScore,
        decimal trirScore,
        bool oshaTrainingCompliant,
        int deficiencyCount,
        UtcTimestamp insuranceExpiresAtUtc,
        UtcTimestamp vettedAtUtc,
        int validityPeriodDays,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(PreQualificationVettingStatus.UnderReview);

        if (!oshaTrainingCompliant)
            throw new PreQualificationVettingInvalidStatusTransitionException(PreQualificationVettingId, Status.ToString(), PreQualificationVettingStatus.Approved.ToString(), correlationId, causationId);
        if (validityPeriodDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(validityPeriodDays), "Validity period must be positive.");

        EmrScore               = emrScore;
        TrirScore               = trirScore;
        OshaTrainingCompliant   = oshaTrainingCompliant;
        DeficiencyCount         = deficiencyCount;
        InsuranceExpiresAtUtc   = insuranceExpiresAtUtc;
        VettedAtUtc             = vettedAtUtc;
        ExpiresAtUtc            = new UtcTimestamp(vettedAtUtc.Value.AddDays(validityPeriodDays));
        Status                  = PreQualificationVettingStatus.Approved;

        AddDomainEvent(new PreQualificationVettingApprovedEvent(
            PreQualificationVettingEventId.New(), PreQualificationVettingId, emrScore, trirScore, ExpiresAtUtc.Value,
            vettedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Reject -----------------------------------------------------------

    /// <summary>Rejects the contractor for failing to meet minimum safety/compliance standards.</summary>
    public void Reject(string reason, UtcTimestamp vettedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PreQualificationVettingStatus.UnderReview);

        RejectionReason = reason;
        VettedAtUtc = vettedAtUtc;
        Status = PreQualificationVettingStatus.Rejected;

        AddDomainEvent(new PreQualificationVettingRejectedEvent(
            PreQualificationVettingEventId.New(), PreQualificationVettingId, reason, vettedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Expire -----------------------------------------------------------

    /// <summary>Expires this approval once its validity period elapses, requiring annual re-vetting.</summary>
    public void Expire(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PreQualificationVettingStatus.Approved);

        Status = PreQualificationVettingStatus.Expired;

        AddDomainEvent(new PreQualificationVettingExpiredEvent(
            PreQualificationVettingEventId.New(), PreQualificationVettingId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPreQualificationVettingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PreQualificationVettingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PreQualificationVetting {PreQualificationVettingId}.");
    }
}
