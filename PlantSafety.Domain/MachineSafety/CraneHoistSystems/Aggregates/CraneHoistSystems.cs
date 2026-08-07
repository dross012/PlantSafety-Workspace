// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         MachineSafety.CraneHoistSystems
//  Layer:          Domain
//  File:           CraneHoistSystems.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single crane or hoist asset: dedicated crane/hoist
//                  equipment safety management covering asset registration, load-limit
//                  enforcement (lift events and overload detection), the mandatory periodic
//                  inspection/certification cycle, operator qualification, and rigging safety
//                  inspections. Distinct from the general MachineGuardsBarriers/SafetyValvesRelays
//                  sub-components; this aggregate models the crane/hoist asset's full operating
//                  lifecycle end-to-end.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.MachineSafety.CraneHoistSystems.Aggregates;

/// <summary>Aggregate root for a single crane or hoist asset.</summary>
public sealed partial class CraneHoistSystems
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this CraneHoistSystems aggregate.</summary>
    public CraneHoistSystemsId CraneHoistSystemsId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public CraneHoistSystemsStatus Status { get; private set; }

    // ---- Asset identity ------------------------------------------------------

    /// <summary>Equipment tag for this crane/hoist asset.</summary>
    public string AssetTag { get; private set; } = string.Empty;

    /// <summary>Human-readable title of this crane/hoist asset.</summary>
    public string AssetTitle { get; private set; } = string.Empty;

    /// <summary>Crane/hoist asset class.</summary>
    public CraneHoistAssetType AssetType { get; private set; }

    /// <summary>Manufacturer-rated maximum load capacity, in kilograms.</summary>
    public decimal RatedLoadCapacityKg { get; private set; }

    // ---- Load limit enforcement ------------------------------------------------

    /// <summary>Most recently observed lift load, in kilograms.</summary>
    public decimal? LastObservedLoadKg { get; private set; }

    /// <summary>UTC timestamp of the most recent lift event, if recorded.</summary>
    public UtcTimestamp? LastLiftAtUtc { get; private set; }

    /// <summary>Count of recorded overload events (observed load exceeding rated capacity) over the asset's lifetime.</summary>
    public int OverloadEventCount { get; private set; }

    /// <summary>Whether the asset currently has an unresolved overload flag.</summary>
    public bool OverloadFlagged { get; private set; }

    // ---- Inspection & certification ------------------------------------------------

    /// <summary>UTC due date of the next scheduled inspection, if scheduled.</summary>
    public UtcTimestamp? InspectionDueAtUtc { get; private set; }

    /// <summary>UTC timestamp of the most recently completed inspection, if recorded.</summary>
    public UtcTimestamp? LastInspectionAtUtc { get; private set; }

    /// <summary>Result of the most recently completed inspection, if recorded.</summary>
    public bool? LastInspectionPassed { get; private set; }

    // ---- Operator qualification ------------------------------------------------

    /// <summary>Identity of the currently qualified operator, if any.</summary>
    public UserId OperatorUserId { get; private set; }

    /// <summary>UTC timestamp the current operator qualification was granted, if any.</summary>
    public UtcTimestamp? OperatorQualifiedAtUtc { get; private set; }

    /// <summary>UTC expiry timestamp of the current operator qualification, if any.</summary>
    public UtcTimestamp? OperatorQualificationExpiresAtUtc { get; private set; }

    /// <summary>Whether the current operator's qualification is suspended.</summary>
    public bool OperatorQualificationSuspended { get; private set; }

    // ---- Rigging safety ------------------------------------------------

    /// <summary>UTC timestamp of the most recently recorded rigging inspection, if any.</summary>
    public UtcTimestamp? LastRiggingInspectionAtUtc { get; private set; }

    /// <summary>Whether the most recently recorded rigging inspection detected a defect.</summary>
    public bool? LastRiggingDefectDetected { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ICraneHoistSystemsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ICraneHoistSystemsDomainEvent> DomainEvents
        => new ReadOnlyCollection<ICraneHoistSystemsDomainEvent>(_domainEvents);

    // For rehydration only.
    private CraneHoistSystems() { }

    // ---- Factory: Create (Register) -----------------------------------------------------------

    /// <summary>Registers a new crane/hoist asset and raises <see cref="CraneHoistAssetRegisteredEvent"/>.</summary>
    public static CraneHoistSystems Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string assetTag = "",
        string assetTitle = "",
        CraneHoistAssetType assetType = CraneHoistAssetType.OverheadBridgeCrane,
        decimal ratedLoadCapacityKg = 0m)
    {
        var agg = new CraneHoistSystems
        {
            CraneHoistSystemsId = CraneHoistSystemsId.New(),
            TenantId            = tenantId,
            SiteId              = siteId,
            Status              = CraneHoistSystemsStatus.Registered,
            AssetTag            = (assetTag ?? string.Empty).Trim(),
            AssetTitle          = (assetTitle ?? string.Empty).Trim(),
            AssetType           = assetType,
            RatedLoadCapacityKg = ratedLoadCapacityKg,
            // OperatorUserId is NOT NULL in storage and has no distinct value at registration
            // time (QualifyOperator assigns the real operator later) -- default to the creator,
            // matching this codebase's placeholder convention rather than leaving UserId's
            // struct default (Guid.Empty), which bypasses UserId's constructor guard in memory
            // but fails UserId.FromGuid on rehydration from the database.
            OperatorUserId      = createdBy,
            CreatedAtUtc        = createdAtUtc,
            CreatedBy           = createdBy,
        };

        agg.AddDomainEvent(new CraneHoistAssetRegisteredEvent(
            CraneHoistSystemsEventId.New(), agg.CraneHoistSystemsId, assetType, ratedLoadCapacityKg, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordLiftEvent -----------------------------------------------

    /// <summary>Records a lift event (a completed lift and its observed load) for this asset.</summary>
    public void RecordLiftEvent(decimal observedLoadKg, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureNotRetired();

        LastObservedLoadKg = observedLoadKg;
        LastLiftAtUtc       = occurredAtUtc;

        AddDomainEvent(new LiftEventRecordedEvent(
            CraneHoistSystemsEventId.New(), CraneHoistSystemsId, observedLoadKg, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordOverloadEvent -----------------------------------------------

    /// <summary>Records an overload event: an observed load in excess of the asset's rated load capacity.</summary>
    public void RecordOverloadEvent(decimal observedLoadKg, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureNotRetired();

        LastObservedLoadKg = observedLoadKg;
        OverloadEventCount++;
        OverloadFlagged = true;

        AddDomainEvent(new LoadLimitExceededEvent(
            CraneHoistSystemsEventId.New(), CraneHoistSystemsId, observedLoadKg, RatedLoadCapacityKg, OverloadEventCount, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: ScheduleCraneInspection -----------------------------------------------

    /// <summary>Schedules the next mandatory crane inspection.</summary>
    public void ScheduleCraneInspection(UtcTimestamp dueAtUtc, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(CraneHoistSystemsStatus.Registered, CraneHoistSystemsStatus.InService, CraneHoistSystemsStatus.InspectionOverdue);

        InspectionDueAtUtc = dueAtUtc;
        Status              = CraneHoistSystemsStatus.InspectionDue;

        AddDomainEvent(new CraneInspectionScheduledEvent(
            CraneHoistSystemsEventId.New(), CraneHoistSystemsId, dueAtUtc, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: CompleteCraneInspection -----------------------------------------------

    /// <summary>Records the outcome of a completed crane inspection.</summary>
    public void CompleteCraneInspection(bool passed, UtcTimestamp completedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(CraneHoistSystemsStatus.InspectionDue, CraneHoistSystemsStatus.InspectionOverdue);

        var wasOverdue = InspectionDueAtUtc is not null && completedAtUtc.Value > InspectionDueAtUtc.Value.Value;

        LastInspectionAtUtc    = completedAtUtc;
        LastInspectionPassed   = passed;
        Status                  = passed ? CraneHoistSystemsStatus.InService : CraneHoistSystemsStatus.InspectionOverdue;

        AddDomainEvent(new CraneInspectionCompletedEvent(
            CraneHoistSystemsEventId.New(), CraneHoistSystemsId, passed, completedAtUtc, actor, correlationId, causationId));

        if (wasOverdue)
        {
            AddDomainEvent(new CraneInspectionOverdueEvent(
                CraneHoistSystemsEventId.New(), CraneHoistSystemsId, InspectionDueAtUtc!.Value, completedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: QualifyOperator -----------------------------------------------

    /// <summary>Qualifies an operator to run this crane/hoist asset.</summary>
    public void QualifyOperator(UserId operatorUserId, UtcTimestamp qualifiedAtUtc, UtcTimestamp? expiresAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureNotRetired();

        OperatorUserId                      = operatorUserId;
        OperatorQualifiedAtUtc              = qualifiedAtUtc;
        OperatorQualificationExpiresAtUtc   = expiresAtUtc;
        OperatorQualificationSuspended      = false;

        AddDomainEvent(new OperatorQualifiedEvent(
            CraneHoistSystemsEventId.New(), CraneHoistSystemsId, operatorUserId, expiresAtUtc, qualifiedAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: SuspendOperatorQualification -----------------------------------------------

    /// <summary>Suspends the current operator's qualification to run this crane/hoist asset.</summary>
    public void SuspendOperatorQualification(string reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        OperatorQualificationSuspended = true;

        AddDomainEvent(new OperatorQualificationSuspendedEvent(
            CraneHoistSystemsEventId.New(), CraneHoistSystemsId, OperatorUserId, (reason ?? string.Empty).Trim(), occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: RecordRiggingInspection -----------------------------------------------

    /// <summary>Records the outcome of a rigging safety inspection (slings, shackles, hooks, chains).</summary>
    public void RecordRiggingInspection(bool defectDetected, string notes, UtcTimestamp inspectedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureNotRetired();

        LastRiggingInspectionAtUtc = inspectedAtUtc;
        LastRiggingDefectDetected  = defectDetected;

        if (defectDetected)
        {
            AddDomainEvent(new RiggingDefectDetectedEvent(
                CraneHoistSystemsEventId.New(), CraneHoistSystemsId, (notes ?? string.Empty).Trim(), inspectedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: Retire -----------------------------------------------------------

    /// <summary>Retires the asset from the crane/hoist registry.</summary>
    public void Retire(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (Status == CraneHoistSystemsStatus.Retired) return;

        Status = CraneHoistSystemsStatus.Retired;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ICraneHoistSystemsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureNotRetired()
    {
        if (Status == CraneHoistSystemsStatus.Retired)
            throw new InvalidOperationException(
                $"CraneHoistSystems {CraneHoistSystemsId} is retired and no longer accepts operations.");
    }

    private void EnsureStatusIsOneOf(params CraneHoistSystemsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for CraneHoistSystems {CraneHoistSystemsId}.");
    }
}
