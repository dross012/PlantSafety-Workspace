// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.Ppe
//  Layer:          Domain
//  File:           Ppe.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single Personal Protective Equipment (PPE) issuance record.
//                  PPE serves as the final line of defense against workplace hazards. Models the real
//                  issuance lifecycle: issuance to an employee, fit testing, periodic inspection,
//                  replacement flagging, and retirement, per the PlantSafety Program Structure PPE
//                  component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.Ppe.Aggregates;

/// <summary>Aggregate root for a single PPE issuance record: the final line of defense against workplace hazards.</summary>
public sealed partial class Ppe
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Ppe aggregate.</summary>
    public PpeId PpeId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PpeStatus Status { get; private set; }

    // ---- Item identity -------------------------------------------------------

    /// <summary>Human-readable item description (e.g. "Cut-Resistant Gloves - Level A4").</summary>
    public string ItemDescription { get; private set; } = string.Empty;

    /// <summary>PPE hazard category.</summary>
    public PpeCategory Category { get; private set; }

    /// <summary>Plant zone/area this PPE requirement applies to.</summary>
    public string AssignedZone { get; private set; } = string.Empty;

    /// <summary>Identity of the employee this item was issued to.</summary>
    public UserId IssuedToUserId { get; private set; }

    /// <summary>UTC timestamp the item was issued.</summary>
    public UtcTimestamp IssuedAtUtc { get; private set; }

    /// <summary>Whether this PPE category requires a fit test (e.g. respirators, certain gloves).</summary>
    public bool RequiresFitTest { get; private set; }

    /// <summary>UTC timestamp the fit test was passed, if applicable and recorded.</summary>
    public UtcTimestamp? FitTestedAtUtc { get; private set; }

    /// <summary>Replacement cycle length, in days, for this PPE category.</summary>
    public int ReplacementCycleDays { get; private set; } = 180;

    /// <summary>UTC timestamp the item is next due for replacement.</summary>
    public UtcTimestamp? NextReplacementDueAtUtc { get; private set; }

    /// <summary>UTC timestamp of the most recent condition inspection, if any.</summary>
    public UtcTimestamp? LastInspectedAtUtc { get; private set; }

    /// <summary>Condition score (0-100) from the most recent inspection.</summary>
    public decimal? ConditionScore { get; private set; }

    /// <summary>Narrative reason recorded when flagged for replacement.</summary>
    public string? ReplacementReason { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPpeDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPpeDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPpeDomainEvent>(_domainEvents);

    // For rehydration only.
    private Ppe() { }

    // ---- Factory: Issue (Create) --------------------------------------------

    /// <summary>Issues a new PPE item to an employee and raises <see cref="PpeIssuedEvent"/>.</summary>
    public static Ppe Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string itemDescription,
        PpeCategory category,
        string assignedZone,
        UserId issuedToUserId,
        bool requiresFitTest,
        int replacementCycleDays = 180)
    {
        if (string.IsNullOrWhiteSpace(itemDescription))
            throw new ArgumentException("Item description is required.", nameof(itemDescription));
        if (replacementCycleDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(replacementCycleDays), "Replacement cycle must be positive.");

        var agg = new Ppe
        {
            PpeId                    = PpeId.New(),
            TenantId                 = tenantId,
            SiteId                   = siteId,
            Status                   = requiresFitTest ? PpeStatus.PendingFitTest : PpeStatus.InService,
            ItemDescription          = itemDescription.Trim(),
            Category                 = category,
            AssignedZone             = (assignedZone ?? string.Empty).Trim(),
            IssuedToUserId           = issuedToUserId,
            IssuedAtUtc              = createdAtUtc,
            RequiresFitTest          = requiresFitTest,
            ReplacementCycleDays     = replacementCycleDays,
            NextReplacementDueAtUtc  = new UtcTimestamp(createdAtUtc.Value.AddDays(replacementCycleDays)),
            CreatedAtUtc             = createdAtUtc,
            CreatedBy                = createdBy,
        };

        agg.AddDomainEvent(new PpeIssuedEvent(
            PpeEventId.New(), agg.PpeId, category, issuedToUserId, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordFitTest -------------------------------------------------

    /// <summary>Records the result of a required fit test.</summary>
    public void RecordFitTest(bool passed, UtcTimestamp testedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PpeStatus.PendingFitTest);

        if (!passed)
            throw new PpeInvalidStatusTransitionException(PpeId, Status.ToString(), PpeStatus.InService.ToString(), correlationId, causationId);

        FitTestedAtUtc = testedAtUtc;
        Status = PpeStatus.InService;

        AddDomainEvent(new PpeFitTestRecordedEvent(
            PpeEventId.New(), PpeId, testedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordInspection -----------------------------------------------

    /// <summary>Records the outcome of a periodic wear/damage/contamination inspection.</summary>
    public void RecordInspection(decimal conditionScore, UtcTimestamp inspectedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PpeStatus.InService);

        if (conditionScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(conditionScore), "Condition score must be between 0 and 100.");

        LastInspectedAtUtc = inspectedAtUtc;
        ConditionScore     = conditionScore;

        AddDomainEvent(new PpeInspectionRecordedEvent(
            PpeEventId.New(), PpeId, conditionScore, inspectedAtUtc, actor, correlationId, causationId));

        if (conditionScore < 50)
        {
            FlagForReplacement("Condition score fell below acceptable threshold during inspection.", inspectedAtUtc, actor, correlationId, causationId);
        }
    }

    // ---- Behavior: FlagForReplacement ----------------------------------------------

    /// <summary>Flags this item for replacement due to wear, damage, contamination, or expiry.</summary>
    public void FlagForReplacement(string reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(PpeStatus.InService);

        ReplacementReason = reason;
        Status = PpeStatus.FlaggedForReplacement;

        AddDomainEvent(new PpeFlaggedForReplacementEvent(
            PpeEventId.New(), PpeId, reason, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Retire ------------------------------------------------------------

    /// <summary>Retires this PPE item (replaced or permanently withdrawn from service).</summary>
    public void Retire(UtcTimestamp retiredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == PpeStatus.Retired)
            throw new PpeInvalidStatusTransitionException(PpeId, Status.ToString(), PpeStatus.Retired.ToString(), correlationId, causationId);

        Status = PpeStatus.Retired;

        AddDomainEvent(new PpeRetiredEvent(
            PpeEventId.New(), PpeId, retiredAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPpeDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PpeStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Ppe {PpeId}.");
    }
}
