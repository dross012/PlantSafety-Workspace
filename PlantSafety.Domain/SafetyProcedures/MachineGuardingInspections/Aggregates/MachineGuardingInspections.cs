// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MachineGuardingInspections
//  Layer:          Domain
//  File:           MachineGuardingInspections.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single machine guarding inspection event (a scheduled
//                  execution of the routine inspection procedure), distinct from the
//                  EmployeeSafety/MachineSafety guard-record aggregates. Verifies guards are securely
//                  mounted, interlocks function correctly, and no guards have been bypassed or removed.
//                  Models the real lifecycle: scheduling, execution (pass/deficiency), corrective
//                  action, and closure.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Aggregates;

/// <summary>Aggregate root for a single machine guarding inspection event.</summary>
public sealed partial class MachineGuardingInspections
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this MachineGuardingInspections aggregate.</summary>
    public MachineGuardingInspectionsId MachineGuardingInspectionsId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public MachineGuardingInspectionsStatus Status { get; private set; }

    // ---- Inspection identity ---------------------------------------------------------

    /// <summary>Human-readable title of this inspection.</summary>
    public string InspectionTitle { get; private set; } = string.Empty;

    /// <summary>Inspection check category.</summary>
    public MachineGuardingInspectionCategory InspectionCategory { get; private set; }

    /// <summary>Location of the equipment being inspected.</summary>
    public string EquipmentLocation { get; private set; } = string.Empty;

    /// <summary>Identity of the inspector.</summary>
    public UserId InspectedByUserId { get; private set; }

    /// <summary>UTC timestamp the inspection was executed, if any.</summary>
    public UtcTimestamp? InspectedAtUtc { get; private set; }

    /// <summary>UTC timestamp the next inspection is due.</summary>
    public UtcTimestamp? NextInspectionDueAtUtc { get; private set; }

    /// <summary>Number of deficiencies logged during execution.</summary>
    public int DeficiencyCount { get; private set; }

    /// <summary>Compliance score (0-100) from execution.</summary>
    public decimal? ComplianceScore { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IMachineGuardingInspectionsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IMachineGuardingInspectionsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IMachineGuardingInspectionsDomainEvent>(_domainEvents);

    // For rehydration only.
    private MachineGuardingInspections() { }

    // ---- Factory: Create (Schedule) -----------------------------------------------------------

    /// <summary>Schedules a new machine guarding inspection and raises <see cref="MachineGuardingInspectionScheduledEvent"/>.</summary>
    public static MachineGuardingInspections Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string inspectionTitle = "",
        MachineGuardingInspectionCategory inspectionCategory = MachineGuardingInspectionCategory.GuardMountingCheck,
        string equipmentLocation = "")
    {
        var agg = new MachineGuardingInspections
        {
            MachineGuardingInspectionsId = MachineGuardingInspectionsId.New(),
            TenantId            = tenantId,
            SiteId              = siteId,
            Status              = MachineGuardingInspectionsStatus.Scheduled,
            InspectionTitle     = (inspectionTitle ?? string.Empty).Trim(),
            InspectionCategory  = inspectionCategory,
            EquipmentLocation   = (equipmentLocation ?? string.Empty).Trim(),
            CreatedAtUtc        = createdAtUtc,
            CreatedBy           = createdBy,
        };

        agg.AddDomainEvent(new MachineGuardingInspectionProcedureScheduledEvent(
            MachineGuardingInspectionsEventId.New(), agg.MachineGuardingInspectionsId, inspectionCategory, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: ExecuteInspection -----------------------------------------------

    /// <summary>Records the execution of the scheduled inspection.</summary>
    public void ExecuteInspection(int deficiencyCount, decimal complianceScore, UserId inspectedByUserId, UtcTimestamp inspectedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MachineGuardingInspectionsStatus.Scheduled);

        if (deficiencyCount < 0)
            throw new ArgumentOutOfRangeException(nameof(deficiencyCount), "Deficiency count cannot be negative.");
        if (complianceScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(complianceScore), "Compliance score must be between 0 and 100.");

        DeficiencyCount     = deficiencyCount;
        ComplianceScore     = complianceScore;
        InspectedByUserId   = inspectedByUserId;
        InspectedAtUtc      = inspectedAtUtc;

        if (deficiencyCount == 0)
        {
            Status = MachineGuardingInspectionsStatus.Passed;
            AddDomainEvent(new MachineGuardingInspectionProcedurePassedEvent(
                MachineGuardingInspectionsEventId.New(), MachineGuardingInspectionsId, complianceScore, inspectedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            Status = MachineGuardingInspectionsStatus.DeficiencyLogged;
            AddDomainEvent(new MachineGuardingInspectionDeficiencyLoggedEvent(
                MachineGuardingInspectionsEventId.New(), MachineGuardingInspectionsId, deficiencyCount, inspectedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: BeginCorrectiveAction -----------------------------------------------

    /// <summary>Begins corrective action to remediate logged deficiencies.</summary>
    public void BeginCorrectiveAction(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MachineGuardingInspectionsStatus.DeficiencyLogged);

        Status = MachineGuardingInspectionsStatus.CorrectiveActionInProgress;

        AddDomainEvent(new MachineGuardingInspectionCorrectiveActionStartedEvent(
            MachineGuardingInspectionsEventId.New(), MachineGuardingInspectionsId, occurredAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Close -----------------------------------------------------------

    /// <summary>Verifies corrective action complete and closes out the inspection, scheduling the next cycle.</summary>
    public void Close(UtcTimestamp nextDueAtUtc, UtcTimestamp closedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(MachineGuardingInspectionsStatus.Passed, MachineGuardingInspectionsStatus.CorrectiveActionInProgress);

        NextInspectionDueAtUtc = nextDueAtUtc;
        Status = MachineGuardingInspectionsStatus.Closed;

        AddDomainEvent(new MachineGuardingInspectionClosedEvent(
            MachineGuardingInspectionsEventId.New(), MachineGuardingInspectionsId, nextDueAtUtc, closedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IMachineGuardingInspectionsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params MachineGuardingInspectionsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for MachineGuardingInspections {MachineGuardingInspectionsId}.");
    }
}
