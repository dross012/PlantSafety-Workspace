// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.LotoSystems
//  Layer:          Domain
//  File:           LotoSystems.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single machine-level LOTO isolation system: the padlocks,
//                  hasps, tags, and labeled isolation points attached to a specific piece of equipment.
//                  Distinct from EmployeeSafety.LotoPrograms (the procedure-instance covering one
//                  maintenance job); this aggregate models the equipment's permanent isolation
//                  infrastructure and its zero-energy verification/annual-audit lifecycle.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.LotoSystems.Aggregates;

/// <summary>Aggregate root for a single machine-level LOTO isolation system.</summary>
public sealed partial class LotoSystems
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this LotoSystems aggregate.</summary>
    public LotoSystemsId LotoSystemsId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public LotoSystemsStatus Status { get; private set; }

    // ---- System identity ---------------------------------------------------------

    /// <summary>Human-readable title of this LOTO system.</summary>
    public string SystemTitle { get; private set; } = string.Empty;

    /// <summary>Equipment tag this system isolates.</summary>
    public string EquipmentTag { get; private set; } = string.Empty;

    /// <summary>Hazardous energy source(s) this system isolates.</summary>
    public MachineEnergySourceType EnergySourceTypes { get; private set; }

    /// <summary>Narrative description of the labeled isolation points.</summary>
    public string IsolationPointDescription { get; private set; } = string.Empty;

    /// <summary>Number of isolation points documented.</summary>
    public int IsolationPointCount { get; private set; }

    /// <summary>Identity of the employee who verified zero-energy state.</summary>
    public UserId VerifiedByUserId { get; private set; }

    /// <summary>UTC timestamp zero-energy state was verified, if recorded.</summary>
    public UtcTimestamp? VerifiedZeroEnergyAtUtc { get; private set; }

    /// <summary>UTC timestamp of the most recent mandatory annual audit, if recorded.</summary>
    public UtcTimestamp? AuditedAtUtc { get; private set; }

    /// <summary>Compliance score (0-100) from the most recent audit.</summary>
    public decimal? ComplianceScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ILotoSystemsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ILotoSystemsDomainEvent> DomainEvents
        => new ReadOnlyCollection<ILotoSystemsDomainEvent>(_domainEvents);

    // For rehydration only.
    private LotoSystems() { }

    // ---- Factory: Create (Document) -----------------------------------------------------------

    /// <summary>Documents a new machine-level LOTO system and raises <see cref="LotoSystemDocumentedEvent"/>.</summary>
    public static LotoSystems Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string systemTitle = "",
        string equipmentTag = "",
        MachineEnergySourceType energySourceTypes = MachineEnergySourceType.Electrical,
        string isolationPointDescription = "",
        int isolationPointCount = 1)
    {
        var agg = new LotoSystems
        {
            LotoSystemsId              = LotoSystemsId.New(),
            TenantId                   = tenantId,
            SiteId                     = siteId,
            Status                     = LotoSystemsStatus.Documented,
            SystemTitle                = (systemTitle ?? string.Empty).Trim(),
            EquipmentTag               = (equipmentTag ?? string.Empty).Trim(),
            EnergySourceTypes          = energySourceTypes,
            IsolationPointDescription  = (isolationPointDescription ?? string.Empty).Trim(),
            IsolationPointCount        = isolationPointCount,
            CreatedAtUtc               = createdAtUtc,
            CreatedBy                  = createdBy,
        };

        agg.AddDomainEvent(new LotoSystemDocumentedEvent(
            LotoSystemsEventId.New(), agg.LotoSystemsId, energySourceTypes, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ApplyIsolation -----------------------------------------------

    /// <summary>Applies padlocks/hasps/tags to all isolation points.</summary>
    public void ApplyIsolation(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(LotoSystemsStatus.Documented, LotoSystemsStatus.Released);

        Status = LotoSystemsStatus.IsolationApplied;

        AddDomainEvent(new LotoSystemIsolationAppliedEvent(
            LotoSystemsEventId.New(), LotoSystemsId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: VerifyZeroEnergy -----------------------------------------------

    /// <summary>Verifies zero-energy state.</summary>
    public void VerifyZeroEnergy(UserId verifiedByUserId, UtcTimestamp verifiedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(LotoSystemsStatus.IsolationApplied);

        VerifiedByUserId        = verifiedByUserId;
        VerifiedZeroEnergyAtUtc = verifiedAtUtc;
        Status                  = LotoSystemsStatus.ZeroEnergyVerified;

        AddDomainEvent(new LotoSystemZeroEnergyVerifiedEvent(
            LotoSystemsEventId.New(), LotoSystemsId, verifiedByUserId, verifiedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Release -----------------------------------------------------------

    /// <summary>Releases isolation, returning the machine to service.</summary>
    public void Release(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(LotoSystemsStatus.ZeroEnergyVerified);

        Status = LotoSystemsStatus.Released;

        AddDomainEvent(new LotoSystemReleasedEvent(
            LotoSystemsEventId.New(), LotoSystemsId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordAnnualAudit -----------------------------------------------

    /// <summary>Records the outcome of the mandatory annual LOTO system audit.</summary>
    public void RecordAnnualAudit(bool compliant, decimal complianceScore, UtcTimestamp auditedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (complianceScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(complianceScore), "Compliance score must be between 0 and 100.");

        AuditedAtUtc    = auditedAtUtc;
        ComplianceScore = complianceScore;

        if (!compliant)
            Status = LotoSystemsStatus.AuditFlagged;

        AddDomainEvent(new LotoSystemAuditRecordedEvent(
            LotoSystemsEventId.New(), LotoSystemsId, compliant, complianceScore, auditedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ILotoSystemsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params LotoSystemsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for LotoSystems {LotoSystemsId}.");
    }
}
