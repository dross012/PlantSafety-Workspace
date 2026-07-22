// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           SafetyPolicy.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21 -- replaced the generic single-Create() placeholder with the real
//                  governance lifecycle documented in the Safety Policies Module: draft authoring,
//                  approval chain, publication, stakeholder acknowledgment tracking, mandated
//                  periodic review cadence, and retirement. Grounded in
//                  docs/reference/PlantSafety_Program_Structure_extract.txt (Safety Policies Module,
//                  17 policy categories, "revision history, training requirements, enforcement
//                  rules" line).
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the SafetyPolicies bounded context. Tracks a single governing
//                  policy record: its category, scope, approval chain, publication window, mandated
//                  review cadence, and stakeholder acknowledgment tracking.
//                  Additive-only evolution.
// ============================================================================
using System.Collections.ObjectModel;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Common.Abstractions;
using PlantSafety.Domain.SafetyPolicies.Entities;
using PlantSafety.Domain.SafetyPolicies.Enums;
using PlantSafety.Domain.SafetyPolicies.Events;
using PlantSafety.Domain.SafetyPolicies.Exceptions;
using PlantSafety.Domain.SafetyPolicies.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPolicies.Aggregates;

/// <summary>Aggregate root for the SafetyPolicies bounded context.</summary>
public sealed partial class SafetyPolicy
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this SafetyPolicy aggregate.</summary>
    public SafetyPolicyId SafetyPolicyId { get; private set; }

    /// <summary>Tenant that owns this policy record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this policy is scoped to. Null means enterprise-wide applicability.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current governance lifecycle status.</summary>
    public SafetyPolicyStatus Status { get; private set; }

    // ---- Policy identity & applicability ------------------------------------

    /// <summary>Human-readable policy title (e.g. "Lockout/Tagout Policy").</summary>
    public string SafetyPolicyTitle { get; private set; } = string.Empty;

    /// <summary>Stable, human-assigned policy number/code used on printed/posted copies.</summary>
    public string PolicyNumber { get; private set; } = string.Empty;

    /// <summary>Monotonically increasing policy revision number, starting at 1. Bumped on every completed review.</summary>
    public int SafetyPolicyVersion { get; private set; } = 1;

    /// <summary>Governing category for this policy, drawn from the 17-category PolicyCategory taxonomy.</summary>
    public PolicyCategory Category { get; private set; } = PolicyCategory.Default;

    /// <summary>Free-form description of who this policy applies to (employees, contractors, visitors, specific roles).</summary>
    public string Applicability { get; private set; } = "All Employees";

    /// <summary>Identity of the accountable policy owner (typically the Safety Manager for the category).</summary>
    public UserId OwnerId { get; private set; }

    // ---- Approval chain ------------------------------------------------------

    /// <summary>Identity of the user who approved the current/most recent revision, if any.</summary>
    public UserId? ApprovedBy { get; private set; }

    /// <summary>UTC instant the current/most recent revision was approved, if any.</summary>
    public UtcTimestamp? ApprovedAtUtc { get; private set; }

    // ---- Publication & review cadence ----------------------------------------

    /// <summary>UTC timestamp this policy revision became/becomes effective. Set on Publish().</summary>
    public UtcTimestamp? EffectiveAtUtc { get; private set; }

    /// <summary>Mandated cadence at which this policy must be formally re-reviewed.</summary>
    public ReviewCadence ReviewCadence { get; private set; } = ReviewCadence.Default;

    /// <summary>UTC instant of the most recently completed formal review, if any.</summary>
    public UtcTimestamp? LastReviewedAtUtc { get; private set; }

    /// <summary>UTC instant the next formal review is due. Null until first publication.</summary>
    public DateTime? NextReviewDueUtc { get; private set; }

    // ---- Acknowledgment tracking -----------------------------------------------

    /// <summary>Indicates whether formal stakeholder acknowledgment is required for this policy.</summary>
    public bool ReviewRequired { get; private set; } = true;

    /// <summary>Target number of stakeholders expected to acknowledge this policy (0 = untracked target).</summary>
    public int RequiredAcknowledgmentCount { get; private set; }

    /// <summary>Count of stakeholders who have acknowledged the current published revision.</summary>
    public int ReviewCount { get; private set; }

    private readonly List<PolicyAcknowledgment> _acknowledgments = [];

    /// <summary>Acknowledgment records captured for the current published revision.</summary>
    public IReadOnlyList<PolicyAcknowledgment> Acknowledgments => _acknowledgments.AsReadOnly();

    // ---- Audit ---------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ISafetyPolicyDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISafetyPolicyDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISafetyPolicyDomainEvent>(_domainEvents);

    // For rehydration only.
    private SafetyPolicy() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new SafetyPolicy aggregate in Draft status and raises <see cref="SafetyPolicyCreatedEvent"/>.</summary>
    public static SafetyPolicy Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string safetyPolicyTitle = "",
        string policyNumber = "",
        PolicyCategory? category = null,
        string applicability = "All Employees",
        UserId ownerId = default,
        ReviewCadence? reviewCadence = null,
        bool reviewRequired = true,
        int requiredAcknowledgmentCount = 0)
    {
        var agg = new SafetyPolicy
        {
            SafetyPolicyId              = SafetyPolicyId.New(),
            TenantId                    = tenantId,
            SiteId                      = siteId,
            Status                      = SafetyPolicyStatus.Draft,
            SafetyPolicyTitle           = (safetyPolicyTitle ?? string.Empty).Trim(),
            PolicyNumber                = (policyNumber ?? string.Empty).Trim(),
            SafetyPolicyVersion         = 1,
            Category                    = category ?? PolicyCategory.Default,
            Applicability               = string.IsNullOrWhiteSpace(applicability) ? "All Employees" : applicability.Trim(),
            OwnerId                     = ownerId,
            ReviewCadence               = reviewCadence ?? ReviewCadence.Default,
            ReviewRequired              = reviewRequired,
            RequiredAcknowledgmentCount = requiredAcknowledgmentCount < 0 ? 0 : requiredAcknowledgmentCount,
            ReviewCount                 = 0,
            CreatedAtUtc                = createdAtUtc,
            CreatedBy                   = createdBy,
        };

        agg.AddDomainEvent(new SafetyPolicyCreatedEvent(
            EventId:       SafetyPolicyEventId.New(),
            SafetyPolicyId: agg.SafetyPolicyId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Approval chain -------------------------------------------

    /// <summary>Submits a Draft policy into the approval chain.</summary>
    public void SubmitForApproval(UserId submittedBy, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != SafetyPolicyStatus.Draft)
            throw new SafetyPolicyInvalidStatusTransitionException(SafetyPolicyId, Status.ToString(), SafetyPolicyStatus.PendingApproval.ToString(), correlationId, causationId);

        Status = SafetyPolicyStatus.PendingApproval;

        AddDomainEvent(new SafetyPolicySubmittedForApprovalEvent(
            SafetyPolicyEventId.New(), SafetyPolicyId, submittedBy, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Approves a policy that is pending approval.</summary>
    public void Approve(UserId approvedBy, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != SafetyPolicyStatus.PendingApproval)
            throw new SafetyPolicyInvalidStatusTransitionException(SafetyPolicyId, Status.ToString(), SafetyPolicyStatus.Approved.ToString(), correlationId, causationId);

        Status = SafetyPolicyStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAtUtc = nowUtc;

        AddDomainEvent(new SafetyPolicyApprovedEvent(
            SafetyPolicyEventId.New(), SafetyPolicyId, approvedBy, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Rejects a policy pending approval, returning it to Draft for rework.</summary>
    public void Reject(UserId rejectedBy, string reason, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != SafetyPolicyStatus.PendingApproval)
            throw new SafetyPolicyInvalidStatusTransitionException(SafetyPolicyId, Status.ToString(), SafetyPolicyStatus.Draft.ToString(), correlationId, causationId);

        Status = SafetyPolicyStatus.Draft;

        AddDomainEvent(new SafetyPolicyRejectedEvent(
            SafetyPolicyEventId.New(), SafetyPolicyId, rejectedBy, reason ?? string.Empty, nowUtc, actor, correlationId, causationId));
    }

    // ---- Lifecycle: Publication & review --------------------------------------

    /// <summary>Publishes an approved policy, making it effective and starting the review-cadence clock.</summary>
    public void Publish(UtcTimestamp effectiveAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != SafetyPolicyStatus.Approved)
            throw new SafetyPolicyInvalidStatusTransitionException(SafetyPolicyId, Status.ToString(), SafetyPolicyStatus.Published.ToString(), correlationId, causationId);

        Status = SafetyPolicyStatus.Published;
        EffectiveAtUtc = effectiveAtUtc;
        LastReviewedAtUtc = effectiveAtUtc;
        NextReviewDueUtc = ReviewCadence.ComputeNextDueUtc(effectiveAtUtc.Value);
        _acknowledgments.Clear();
        ReviewCount = 0;

        AddDomainEvent(new SafetyPolicyPublishedEvent(
            SafetyPolicyEventId.New(), SafetyPolicyId, effectiveAtUtc, NextReviewDueUtc.Value, actor, correlationId, causationId));
    }

    /// <summary>Records a stakeholder's acknowledgment of the currently published policy revision.</summary>
    public void RecordAcknowledgment(UserId userId, UtcTimestamp acknowledgedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != SafetyPolicyStatus.Published)
            throw new SafetyPolicyAcknowledgmentNotAllowedException(SafetyPolicyId, Status.ToString(), correlationId, causationId);

        _acknowledgments.Add(new PolicyAcknowledgment(
            Guid.NewGuid(), SafetyPolicyId, SafetyPolicyVersion, userId, acknowledgedAtUtc));
        ReviewCount++;

        AddDomainEvent(new SafetyPolicyAcknowledgedEvent(
            SafetyPolicyEventId.New(), SafetyPolicyId, userId, SafetyPolicyVersion, ReviewCount, acknowledgedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Starts the mandated periodic review cycle for a Published policy.</summary>
    public void StartReviewCycle(UserId startedBy, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != SafetyPolicyStatus.Published)
            throw new SafetyPolicyInvalidStatusTransitionException(SafetyPolicyId, Status.ToString(), SafetyPolicyStatus.UnderReview.ToString(), correlationId, causationId);

        Status = SafetyPolicyStatus.UnderReview;

        AddDomainEvent(new SafetyPolicyReviewCycleStartedEvent(
            SafetyPolicyEventId.New(), SafetyPolicyId, startedBy, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Completes a periodic review, bumping the revision number and republishing the policy.</summary>
    public void CompleteReview(UserId reviewedBy, UtcTimestamp nowUtc, ReviewCadence? newCadence, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status != SafetyPolicyStatus.UnderReview)
            throw new SafetyPolicyInvalidStatusTransitionException(SafetyPolicyId, Status.ToString(), SafetyPolicyStatus.Published.ToString(), correlationId, causationId);

        SafetyPolicyVersion++;
        ReviewCadence = newCadence ?? ReviewCadence;
        Status = SafetyPolicyStatus.Published;
        LastReviewedAtUtc = nowUtc;
        NextReviewDueUtc = ReviewCadence.ComputeNextDueUtc(nowUtc.Value);
        _acknowledgments.Clear();
        ReviewCount = 0;

        AddDomainEvent(new SafetyPolicyReviewCompletedEvent(
            SafetyPolicyEventId.New(), SafetyPolicyId, reviewedBy, SafetyPolicyVersion, NextReviewDueUtc.Value, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Retires a policy that is Published or UnderReview, ending its governing force.</summary>
    public void Retire(UserId retiredBy, string reason, UtcTimestamp nowUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(SafetyPolicyStatus.Published, SafetyPolicyStatus.UnderReview);

        Status = SafetyPolicyStatus.Retired;

        AddDomainEvent(new SafetyPolicyRetiredEvent(
            SafetyPolicyEventId.New(), SafetyPolicyId, retiredBy, reason ?? string.Empty, nowUtc, actor, correlationId, causationId));
    }

    /// <summary>Archives a Retired policy for long-term retention.</summary>
    public void Archive(CorrelationId correlationId, CausationId causationId)
    {
        if (Status != SafetyPolicyStatus.Retired)
            throw new SafetyPolicyInvalidStatusTransitionException(SafetyPolicyId, Status.ToString(), SafetyPolicyStatus.Archived.ToString(), correlationId, causationId);

        Status = SafetyPolicyStatus.Archived;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ISafetyPolicyDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params SafetyPolicyStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for SafetyPolicy {SafetyPolicyId}.");
    }
}
