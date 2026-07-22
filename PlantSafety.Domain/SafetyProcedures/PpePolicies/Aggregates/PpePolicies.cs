// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePolicies.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single PPE policy document defining when, where, and how
//                  protective equipment must be used: issuance, inspection, enforcement in designated
//                  zones, and replacement schedules. Models the real document lifecycle: drafting,
//                  approval, publication, scheduled revision, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Aggregates;

/// <summary>Aggregate root for a single PPE policy document.</summary>
public sealed partial class PpePolicies
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PpePolicies aggregate.</summary>
    public PpePoliciesId PpePoliciesId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PpePoliciesStatus Status { get; private set; }

    // ---- Policy identity ---------------------------------------------------------

    /// <summary>Human-readable title of this policy document.</summary>
    public string PolicyTitle { get; private set; } = string.Empty;

    /// <summary>PPE category this policy governs.</summary>
    public PolicyPpeCategory PpeCategory { get; private set; }

    /// <summary>Plant zone this policy applies to.</summary>
    public string AssignedZone { get; private set; } = string.Empty;

    /// <summary>Identity of the approver.</summary>
    public UserId ApprovedByUserId { get; private set; }

    /// <summary>UTC timestamp the document was published, if any.</summary>
    public UtcTimestamp? PublishedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next scheduled review is due.</summary>
    public UtcTimestamp? NextReviewDueAtUtc { get; private set; }

    /// <summary>Number of replacement-schedule entries defined under this policy.</summary>
    public int ReplacementScheduleCount { get; private set; }

    /// <summary>Compliance score (0-100) from the most recent enforcement audit.</summary>
    public decimal? ComplianceScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPpePoliciesDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPpePoliciesDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPpePoliciesDomainEvent>(_domainEvents);

    // For rehydration only.
    private PpePolicies() { }

    // ---- Factory: Create (Draft) -----------------------------------------------------------

    /// <summary>Drafts a new PPE policy document and raises <see cref="PpePolicyDraftedEvent"/>.</summary>
    public static PpePolicies Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string policyTitle = "",
        PolicyPpeCategory ppeCategory = PolicyPpeCategory.HeadProtection,
        string assignedZone = "")
    {
        var agg = new PpePolicies
        {
            PpePoliciesId  = PpePoliciesId.New(),
            TenantId       = tenantId,
            SiteId         = siteId,
            Status         = PpePoliciesStatus.Draft,
            PolicyTitle    = (policyTitle ?? string.Empty).Trim(),
            PpeCategory    = ppeCategory,
            AssignedZone   = (assignedZone ?? string.Empty).Trim(),
            CreatedAtUtc   = createdAtUtc,
            CreatedBy      = createdBy,
        };

        agg.AddDomainEvent(new PpePolicyDraftedEvent(
            PpePoliciesEventId.New(), agg.PpePoliciesId, ppeCategory, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: SubmitForApproval -----------------------------------------------

    /// <summary>Submits the draft for approval.</summary>
    public void SubmitForApproval(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PpePoliciesStatus.Draft, PpePoliciesStatus.UnderRevision);

        Status = PpePoliciesStatus.PendingApproval;

        AddDomainEvent(new PpePolicySubmittedForApprovalEvent(
            PpePoliciesEventId.New(), PpePoliciesId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Publish -----------------------------------------------------------

    /// <summary>Approves and publishes the policy, defining its replacement schedule.</summary>
    public void Publish(UserId approvedByUserId, int replacementScheduleCount, UtcTimestamp publishedAtUtc, int reviewCycleDays, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PpePoliciesStatus.PendingApproval);

        if (replacementScheduleCount < 0)
            throw new ArgumentOutOfRangeException(nameof(replacementScheduleCount), "Replacement schedule count cannot be negative.");

        ApprovedByUserId          = approvedByUserId;
        ReplacementScheduleCount  = replacementScheduleCount;
        PublishedAtUtc            = publishedAtUtc;
        NextReviewDueAtUtc        = new UtcTimestamp(publishedAtUtc.Value.AddDays(reviewCycleDays));
        Status                    = PpePoliciesStatus.Published;

        AddDomainEvent(new PpePolicyPublishedEvent(
            PpePoliciesEventId.New(), PpePoliciesId, approvedByUserId, publishedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordEnforcementAudit -----------------------------------------------

    /// <summary>Records the outcome of an enforcement/compliance audit against this policy.</summary>
    public void RecordEnforcementAudit(decimal complianceScore, UtcTimestamp auditedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PpePoliciesStatus.Published);

        if (complianceScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(complianceScore), "Compliance score must be between 0 and 100.");

        ComplianceScore = complianceScore;

        AddDomainEvent(new PpePolicyEnforcementAuditRecordedEvent(
            PpePoliciesEventId.New(), PpePoliciesId, complianceScore, auditedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ScheduleRevision -----------------------------------------------

    /// <summary>Opens a revision cycle for the policy.</summary>
    public void ScheduleRevision(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PpePoliciesStatus.Published);

        Status = PpePoliciesStatus.UnderRevision;

        AddDomainEvent(new PpePolicyRevisionScheduledEvent(
            PpePoliciesEventId.New(), PpePoliciesId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires the policy document.</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == PpePoliciesStatus.Retired)
            throw new PpePoliciesInvalidStatusTransitionException(PpePoliciesId, Status.ToString(), PpePoliciesStatus.Retired.ToString(), correlationId, causationId);

        Status = PpePoliciesStatus.Retired;

        AddDomainEvent(new PpePolicyRetiredEvent(
            PpePoliciesEventId.New(), PpePoliciesId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPpePoliciesDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PpePoliciesStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PpePolicies {PpePoliciesId}.");
    }
}
