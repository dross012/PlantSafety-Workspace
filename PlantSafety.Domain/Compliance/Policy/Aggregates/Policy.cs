// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Domain
//  File:           Policy.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  policy-lifecycle record shape grounded in project_plantsafety_domains.md
//                  Module 13 (Policy: "Safety policy creation, distribution, and
//                  acknowledgment"). Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Policy bounded context. Tracks a single
//                  safety policy document: its title, version, ownership,
//                  effective date, and acknowledgment tracking.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Policy.Aggregates;

/// <summary>Aggregate root for the Policy bounded context.</summary>
public sealed partial class Policy
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Policy aggregate.</summary>
    public PolicyId PolicyId { get; private set; }

    /// <summary>Tenant that owns this policy record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this policy is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PolicyStatus Status { get; private set; }

    // ---- Policy-specific business properties --------------------------------

    /// <summary>Human-readable policy title.</summary>
    public string PolicyTitle { get; private set; } = string.Empty;

    /// <summary>Monotonically increasing policy revision number, starting at 1.</summary>
    public int PolicyVersion { get; private set; } = 1;

    /// <summary>Policy category (e.g., PPE, HotWork, EmergencyResponse, GeneralSafety).</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Identity of the accountable policy owner.</summary>
    public UserId OwnerId { get; private set; }

    /// <summary>UTC timestamp the policy becomes effective.</summary>
    public UtcTimestamp? EffectiveAtUtc { get; private set; }

    /// <summary>Indicates whether personnel acknowledgment is required for this policy.</summary>
    public bool AcknowledgmentRequired { get; private set; } = true;

    /// <summary>Count of personnel who have acknowledged this policy.</summary>
    public int AcknowledgmentCount { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPolicyDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPolicyDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPolicyDomainEvent>(_domainEvents);

    // For rehydration only.
    private Policy() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Policy aggregate and raises <see cref="PolicyCreatedEvent"/>.</summary>
    public static Policy Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string policyTitle = "",
        int policyVersion = 1,
        string category = "",
        UserId ownerId = default,
        UtcTimestamp? effectiveAtUtc = null,
        bool acknowledgmentRequired = true,
        int acknowledgmentCount = 0)
    {
        var agg = new Policy
        {
            PolicyId                = PolicyId.New(),
            TenantId                = tenantId,
            SiteId                  = siteId,
            Status                  = PolicyStatus.Created,
            PolicyTitle             = (policyTitle ?? string.Empty).Trim(),
            PolicyVersion           = policyVersion < 1 ? 1 : policyVersion,
            Category                = (category ?? string.Empty).Trim(),
            OwnerId                 = ownerId,
            EffectiveAtUtc          = effectiveAtUtc,
            AcknowledgmentRequired  = acknowledgmentRequired,
            AcknowledgmentCount     = acknowledgmentCount,
            CreatedAtUtc            = createdAtUtc,
            CreatedBy               = createdBy,
        };

        agg.AddDomainEvent(new PolicyCreatedEvent(
            EventId:       new PolicyEventId(Uuid7.New()),
            PolicyId:      agg.PolicyId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Approve / Acknowledge -------------------------------------

    /// <summary>
    /// Approves the policy and makes it effective (Created -&gt; Active). Valid only from
    /// <see cref="PolicyStatus.Created"/>.
    /// </summary>
    public void Approve(
        UserId approvedBy,
        UtcTimestamp effectiveAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PolicyLifecycleRules.CanActivate, PolicyStatus.Created);

        EffectiveAtUtc = effectiveAtUtc;
        Status = PolicyStatus.Active;

        AddDomainEvent(new PolicyApprovedEvent(
            EventId: new PolicyEventId(Uuid7.New()),
            PolicyId: PolicyId,
            ApprovedByUserId: approvedBy,
            EffectiveAtUtc: effectiveAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Records an acknowledgment of this policy by a person. Does not change lifecycle status.
    /// Valid only while the policy is <see cref="PolicyStatus.Active"/>.
    /// </summary>
    public void RecordAcknowledgment(
        UserId acknowledgedBy,
        UtcTimestamp acknowledgedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PolicyLifecycleRules.CanAcknowledge, PolicyStatus.Active);

        AcknowledgmentCount++;

        AddDomainEvent(new PolicyAcknowledgedEvent(
            EventId: new PolicyEventId(Uuid7.New()),
            PolicyId: PolicyId,
            AcknowledgedByUserId: acknowledgedBy,
            AcknowledgedAtUtc: acknowledgedAtUtc,
            AcknowledgmentCount: AcknowledgmentCount,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Lifecycle: Supersede / Close / Archive -------------------------------

    /// <summary>
    /// Supersedes an active policy with a newer revision (Active -&gt; Completed). Valid only from
    /// <see cref="PolicyStatus.Active"/>.
    /// </summary>
    public void Supersede(
        UserId supersededBy,
        UtcTimestamp supersededAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PolicyLifecycleRules.CanComplete, PolicyStatus.Active);

        Status = PolicyStatus.Completed;

        AddDomainEvent(new PolicySupersededEvent(
            EventId: new PolicyEventId(Uuid7.New()),
            PolicyId: PolicyId,
            SupersededByUserId: supersededBy,
            SupersededAtUtc: supersededAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Formally closes a superseded policy (Completed -&gt; Closed). Valid only from
    /// <see cref="PolicyStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PolicyLifecycleRules.CanClose, PolicyStatus.Completed);

        Status = PolicyStatus.Closed;

        AddDomainEvent(new PolicyClosedEvent(
            EventId: new PolicyEventId(Uuid7.New()),
            PolicyId: PolicyId,
            ClosedByUserId: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed policy for compliance retention (Closed -&gt; Archived). Valid only from
    /// <see cref="PolicyStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, PolicyLifecycleRules.CanArchive, PolicyStatus.Closed);

        Status = PolicyStatus.Archived;

        AddDomainEvent(new PolicyArchivedEvent(
            EventId: new PolicyEventId(Uuid7.New()),
            PolicyId: PolicyId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPolicyDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PolicyStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Policy {PolicyId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<PolicyStatus, bool> guard, PolicyStatus expected)
    {
        if (guard(Status)) return;

        throw new PolicyInvalidStatusTransitionException(
            PolicyId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
