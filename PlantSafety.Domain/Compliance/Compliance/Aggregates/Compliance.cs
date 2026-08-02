// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Domain
//  File:           Compliance.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  regulatory-governance record shape (regulation reference, rule
//                  description, score, non-conformances) grounded in
//                  project_plantsafety_domains.md Module 5 (Compliance: "Regulatory
//                  governance, rule evaluation, and non-conformance workflows").
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Compliance bounded context. Tracks a single
//                  regulatory-compliance evaluation record: the regulation/rule it
//                  evaluates, its computed score, and non-conformance volume.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Compliance.Aggregates;

/// <summary>Aggregate root for the Compliance bounded context.</summary>
public sealed partial class Compliance
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Compliance aggregate.</summary>
    public ComplianceId ComplianceId { get; private set; }

    /// <summary>Tenant that owns this compliance record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this compliance record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ComplianceStatus Status { get; private set; }

    // ---- Compliance-specific business properties ----------------------------

    /// <summary>Regulation/standard reference this record evaluates (e.g., "29 CFR 1910.147").</summary>
    public string RegulationReference { get; private set; } = string.Empty;

    /// <summary>Narrative description of the rule being evaluated.</summary>
    public string RuleDescription { get; private set; } = string.Empty;

    /// <summary>Computed compliance score (0-100) for this rule/regulation.</summary>
    public decimal ComplianceScore { get; private set; }

    /// <summary>Count of open non-conformances tied to this compliance record.</summary>
    public int NonConformanceCount { get; private set; }

    /// <summary>Identity of the party accountable for maintaining compliance.</summary>
    public UserId ResponsiblePartyId { get; private set; }

    /// <summary>UTC timestamp this compliance record was last evaluated.</summary>
    public UtcTimestamp? LastEvaluatedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IComplianceDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IComplianceDomainEvent> DomainEvents
        => new ReadOnlyCollection<IComplianceDomainEvent>(_domainEvents);

    // For rehydration only.
    private Compliance() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Compliance aggregate and raises <see cref="ComplianceCreatedEvent"/>.</summary>
    public static Compliance Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string regulationReference = "",
        string ruleDescription = "",
        decimal complianceScore = 0m,
        int nonConformanceCount = 0,
        UserId responsiblePartyId = default,
        UtcTimestamp? lastEvaluatedAtUtc = null)
    {
        var agg = new Compliance
        {
            ComplianceId          = ComplianceId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = ComplianceStatus.Created,
            RegulationReference   = (regulationReference ?? string.Empty).Trim(),
            RuleDescription       = (ruleDescription ?? string.Empty).Trim(),
            ComplianceScore       = complianceScore,
            NonConformanceCount   = nonConformanceCount,
            ResponsiblePartyId    = responsiblePartyId,
            LastEvaluatedAtUtc    = lastEvaluatedAtUtc,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new ComplianceCreatedEvent(
            EventId:       new ComplianceEventId(Uuid7.New()),
            ComplianceId:      agg.ComplianceId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Lifecycle: Activate / Evaluate / Complete / Close / Archive -------
    // Phase 4 full build-out (2026-08-02): this aggregate previously had only the Create
    // factory. Event names deliberately avoid the bare "Compliance" prefix (e.g.
    // ComplianceRuleActivatedEvent, not ComplianceActivatedEvent) -- see the naming warning in
    // this module's build-out task: "Compliance" collides with both the immediate namespace
    // segment (PlantSafety.Domain.Compliance.Compliance) and the parent module namespace
    // (PlantSafety.Domain.Compliance). Mirrors Compliance.Permits' lifecycle build-out shape.

    /// <summary>
    /// Activates a compliance evaluation record (Created -&gt; Active). Valid only from
    /// <see cref="ComplianceStatus.Created"/>.
    /// </summary>
    public void Activate(
        UserId activatedBy,
        UtcTimestamp activatedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ComplianceLifecycleRules.CanActivate, ComplianceStatus.Created);

        Status = ComplianceStatus.Active;

        AddDomainEvent(new ComplianceRuleActivatedEvent(
            EventId: new ComplianceEventId(Uuid7.New()),
            ComplianceId: ComplianceId,
            ActivatedBy: activatedBy,
            ActivatedAtUtc: activatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Re-evaluates an active compliance record, updating its score and non-conformance count.
    /// Does not change lifecycle status. Valid only from <see cref="ComplianceStatus.Active"/>.
    /// </summary>
    public void Evaluate(
        UserId evaluatedBy,
        decimal complianceScore,
        int nonConformanceCount,
        UtcTimestamp evaluatedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ComplianceLifecycleRules.CanEvaluate, ComplianceStatus.Active);
        if (nonConformanceCount < 0)
            throw new ArgumentOutOfRangeException(nameof(nonConformanceCount), "Non-conformance count must not be negative.");

        ComplianceScore = complianceScore;
        NonConformanceCount = nonConformanceCount;
        LastEvaluatedAtUtc = evaluatedAtUtc;

        AddDomainEvent(new ComplianceRuleEvaluatedEvent(
            EventId: new ComplianceEventId(Uuid7.New()),
            ComplianceId: ComplianceId,
            EvaluatedBy: evaluatedBy,
            ComplianceScore: complianceScore,
            NonConformanceCount: nonConformanceCount,
            EvaluatedAtUtc: evaluatedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Marks the compliance evaluation record completed (Active -&gt; Completed). Valid only from
    /// <see cref="ComplianceStatus.Active"/>.
    /// </summary>
    public void Complete(
        UserId completedBy,
        UtcTimestamp completedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ComplianceLifecycleRules.CanComplete, ComplianceStatus.Active);

        Status = ComplianceStatus.Completed;

        AddDomainEvent(new ComplianceRuleCompletedEvent(
            EventId: new ComplianceEventId(Uuid7.New()),
            ComplianceId: ComplianceId,
            CompletedBy: completedBy,
            CompletedAtUtc: completedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Formally closes a completed compliance record (Completed -&gt; Closed). Valid only from
    /// <see cref="ComplianceStatus.Completed"/>.
    /// </summary>
    public void Close(
        UserId closedBy,
        UtcTimestamp closedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ComplianceLifecycleRules.CanClose, ComplianceStatus.Completed);

        Status = ComplianceStatus.Closed;

        AddDomainEvent(new ComplianceRuleClosedEvent(
            EventId: new ComplianceEventId(Uuid7.New()),
            ComplianceId: ComplianceId,
            ClosedBy: closedBy,
            ClosedAtUtc: closedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    /// <summary>
    /// Archives a closed compliance record for retention (Closed -&gt; Archived). Valid only from
    /// <see cref="ComplianceStatus.Closed"/>.
    /// </summary>
    public void Archive(
        UtcTimestamp archivedAtUtc,
        PerformedByActor actor,
        CorrelationId correlationId,
        CausationId causationId)
    {
        EnsureStatusIsOneOf(correlationId, causationId, ComplianceLifecycleRules.CanArchive, ComplianceStatus.Closed);

        Status = ComplianceStatus.Archived;

        AddDomainEvent(new ComplianceRuleArchivedEvent(
            EventId: new ComplianceEventId(Uuid7.New()),
            ComplianceId: ComplianceId,
            ArchivedAtUtc: archivedAtUtc,
            Actor: actor,
            CorrelationId: correlationId,
            CausationId: causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IComplianceDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ComplianceStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Compliance {ComplianceId}.");
    }

    private void EnsureStatusIsOneOf(CorrelationId correlationId, CausationId causationId, Func<ComplianceStatus, bool> guard, ComplianceStatus expected)
    {
        if (guard(Status)) return;

        throw new ComplianceInvalidStatusTransitionException(
            ComplianceId, Status.ToString(), expected.ToString(), correlationId, causationId);
    }
}
