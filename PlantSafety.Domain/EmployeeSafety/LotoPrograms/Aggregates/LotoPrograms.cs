// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EmployeeSafety.LotoPrograms
//  Layer:          Domain
//  File:           LotoPrograms.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single Lockout/Tagout (LOTO) program instance covering one
//                  equipment isolation cycle. Protects employees during maintenance, repair, or cleaning
//                  by ensuring machines remain fully de-energized: identification of energy sources,
//                  application of locks/hasps/tags to isolation points, verification of zero-energy
//                  state, authorized-employee tracking, and release back to service.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EmployeeSafety.LotoPrograms.Aggregates;

/// <summary>
/// Aggregate root for a single LOTO program instance. LOTO protocols protect employees during
/// maintenance, repair, or cleaning by ensuring machines remain fully de-energized.
/// </summary>
public sealed partial class LotoPrograms
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this LotoPrograms aggregate.</summary>
    public LotoProgramsId LotoProgramsId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public LotoProgramsStatus Status { get; private set; }

    // ---- Program identity -----------------------------------------------------

    /// <summary>Human-readable title of this LOTO program (e.g. "Conveyor Line 4 Motor Replacement").</summary>
    public string ProgramTitle { get; private set; } = string.Empty;

    /// <summary>Asset tag / equipment identifier being isolated.</summary>
    public string EquipmentTag { get; private set; } = string.Empty;

    /// <summary>Hazardous energy source(s) identified for isolation (flags: may combine multiple).</summary>
    public LotoEnergySourceType EnergySourceTypes { get; private set; }

    /// <summary>Narrative description of the isolation points (breakers, valves, disconnects) to be locked.</summary>
    public string IsolationPointDescription { get; private set; } = string.Empty;

    /// <summary>Number of distinct isolation points identified for this program.</summary>
    public int IsolationPointCount { get; private set; }

    // ---- Authorization & isolation state ---------------------------------------

    /// <summary>Identity of the authorized employee who documented/owns this LOTO program.</summary>
    public UserId AuthorizedByUserId { get; private set; }

    /// <summary>Number of locks/hasps/tags physically applied.</summary>
    public int LocksAppliedCount { get; private set; }

    /// <summary>Identity of the employee who verified the zero-energy state.</summary>
    public UserId? VerifiedByUserId { get; private set; }

    /// <summary>UTC timestamp zero-energy state was verified, if recorded.</summary>
    public UtcTimestamp? VerifiedZeroEnergyAtUtc { get; private set; }

    /// <summary>Identity of the employee who released the isolation back to service.</summary>
    public UserId? ReleasedByUserId { get; private set; }

    /// <summary>UTC timestamp the isolation was released, if recorded.</summary>
    public UtcTimestamp? ReleasedAtUtc { get; private set; }

    /// <summary>UTC timestamp of the most recent annual program audit, if recorded.</summary>
    public UtcTimestamp? LastAuditedAtUtc { get; private set; }

    /// <summary>Program compliance score (0-100) from the most recent audit.</summary>
    public decimal? ProgramComplianceScore { get; private set; }

    /// <summary>Narrative notes describing the most recent non-compliance finding, if any.</summary>
    public string? NonComplianceNotes { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ILotoProgramsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ILotoProgramsDomainEvent> DomainEvents
        => new ReadOnlyCollection<ILotoProgramsDomainEvent>(_domainEvents);

    // For rehydration only.
    private LotoPrograms() { }

    // ---- Factory: Document ---------------------------------------------------

    /// <summary>Documents a new LOTO program (energy sources and isolation points identified) and raises <see cref="LotoProgramsDocumentedEvent"/>.</summary>
    public static LotoPrograms Document(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string programTitle,
        string equipmentTag,
        LotoEnergySourceType energySourceTypes,
        string isolationPointDescription,
        int isolationPointCount,
        UserId authorizedByUserId)
    {
        if (string.IsNullOrWhiteSpace(programTitle))
            throw new ArgumentException("Program title is required.", nameof(programTitle));
        if (string.IsNullOrWhiteSpace(equipmentTag))
            throw new ArgumentException("Equipment tag is required.", nameof(equipmentTag));
        if (energySourceTypes == LotoEnergySourceType.None)
            throw new ArgumentException("At least one hazardous energy source must be identified.", nameof(energySourceTypes));
        if (isolationPointCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(isolationPointCount), "At least one isolation point is required.");

        var agg = new LotoPrograms
        {
            LotoProgramsId              = LotoProgramsId.New(),
            TenantId                    = tenantId,
            SiteId                      = siteId,
            Status                      = LotoProgramsStatus.Documented,
            ProgramTitle                = programTitle.Trim(),
            EquipmentTag                = equipmentTag.Trim(),
            EnergySourceTypes           = energySourceTypes,
            IsolationPointDescription   = (isolationPointDescription ?? string.Empty).Trim(),
            IsolationPointCount         = isolationPointCount,
            AuthorizedByUserId          = authorizedByUserId,
            CreatedAtUtc                = createdAtUtc,
            CreatedBy                   = createdBy,
        };

        agg.AddDomainEvent(new LotoProgramsDocumentedEvent(
            LotoProgramsEventId.New(), agg.LotoProgramsId, energySourceTypes, isolationPointCount,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ApplyIsolation ---------------------------------------------

    /// <summary>Records that locks, hasps, and tags have been physically applied to all isolation points.</summary>
    public void ApplyIsolation(int locksAppliedCount, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(LotoProgramsStatus.Documented);

        if (locksAppliedCount < IsolationPointCount)
            throw new LotoProgramsInvalidStatusTransitionException(LotoProgramsId, Status.ToString(), LotoProgramsStatus.IsolationApplied.ToString(), correlationId, causationId);

        LocksAppliedCount = locksAppliedCount;
        Status = LotoProgramsStatus.IsolationApplied;

        AddDomainEvent(new LotoProgramsIsolationAppliedEvent(
            LotoProgramsEventId.New(), LotoProgramsId, locksAppliedCount, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: VerifyZeroEnergy ---------------------------------------------

    /// <summary>Verifies the zero-energy state has been confirmed by an authorized employee.</summary>
    public void VerifyZeroEnergy(UserId verifiedByUserId, UtcTimestamp verifiedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(LotoProgramsStatus.IsolationApplied);

        VerifiedByUserId        = verifiedByUserId;
        VerifiedZeroEnergyAtUtc = verifiedAtUtc;
        Status                  = LotoProgramsStatus.ZeroEnergyVerified;

        AddDomainEvent(new LotoProgramsZeroEnergyVerifiedEvent(
            LotoProgramsEventId.New(), LotoProgramsId, verifiedByUserId, verifiedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Release -------------------------------------------------------

    /// <summary>Removes isolation devices and releases the equipment back to service.</summary>
    public void Release(UserId releasedByUserId, UtcTimestamp releasedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(LotoProgramsStatus.ZeroEnergyVerified);

        ReleasedByUserId = releasedByUserId;
        ReleasedAtUtc    = releasedAtUtc;
        Status           = LotoProgramsStatus.Released;

        AddDomainEvent(new LotoProgramsReleasedEvent(
            LotoProgramsEventId.New(), LotoProgramsId, releasedByUserId, releasedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordAnnualAudit ----------------------------------------------

    /// <summary>Records the outcome of the mandatory annual LOTO program audit.</summary>
    public void RecordAnnualAudit(bool compliant, decimal complianceScore, string? notes, UtcTimestamp auditedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (complianceScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(complianceScore), "Compliance score must be between 0 and 100.");

        LastAuditedAtUtc        = auditedAtUtc;
        ProgramComplianceScore  = complianceScore;

        if (!compliant)
        {
            NonComplianceNotes = notes;
            Status = LotoProgramsStatus.NonCompliant;
        }

        AddDomainEvent(new LotoProgramsAuditRecordedEvent(
            LotoProgramsEventId.New(), LotoProgramsId, compliant, complianceScore, auditedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ILotoProgramsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params LotoProgramsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for LotoPrograms {LotoProgramsId}.");
    }
}
