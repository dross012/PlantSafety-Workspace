// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.ManagementOfChange
//  Layer:          Domain
//  File:           ManagementOfChange.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  Management-of-Change (MOC) record shape. NOTE: no dedicated MOC
//                  field-level spec was found in project_plantsafety_domains.md or
//                  project_plantsafety_overview.md beyond the module's existence
//                  (overview.md Section 6, item 4.46 "MOC") -- properties below use
//                  industry-standard MOC fields (ANSI/ISA-84, OSHA PSM 1910.119(l))
//                  for this entity type. Flagged as unsourced-from-manual.
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the ManagementOfChange bounded context. Tracks a
//                  single change request: its classification, risk level, requester,
//                  approver, and implementation/review schedule.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.ManagementOfChange.Aggregates;

/// <summary>Aggregate root for the ManagementOfChange bounded context.</summary>
public sealed partial class ManagementOfChange
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this ManagementOfChange aggregate.</summary>
    public ManagementOfChangeId ManagementOfChangeId { get; private set; }

    /// <summary>Tenant that owns this change record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this change is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ManagementOfChangeStatus Status { get; private set; }

    // ---- MOC-specific business properties -----------------------------------

    /// <summary>Human-readable title of the proposed change.</summary>
    public string ChangeTitle { get; private set; } = string.Empty;

    /// <summary>Change classification: Process, Equipment, Personnel, Procedure, Software.</summary>
    public string ChangeType { get; private set; } = "Process";

    /// <summary>Business/safety justification for the change.</summary>
    public string Justification { get; private set; } = string.Empty;

    /// <summary>Risk level of the change: Low, Medium, High, Critical.</summary>
    public string RiskLevel { get; private set; } = "Low";

    /// <summary>Identity of the user who requested the change.</summary>
    public UserId RequestedByUserId { get; private set; }

    /// <summary>Identity of the user who approved the change, if approved.</summary>
    public UserId? ApprovedByUserId { get; private set; }

    /// <summary>Approval status: Pending, Approved, Rejected, Deferred.</summary>
    public string ApprovalStatus { get; private set; } = "Pending";

    /// <summary>Planned/actual implementation date for the change.</summary>
    public UtcTimestamp? ImplementationAtUtc { get; private set; }

    /// <summary>Scheduled post-implementation review date.</summary>
    public UtcTimestamp? ReviewDueAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IManagementOfChangeDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IManagementOfChangeDomainEvent> DomainEvents
        => new ReadOnlyCollection<IManagementOfChangeDomainEvent>(_domainEvents);

    // For rehydration only.
    private ManagementOfChange() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new ManagementOfChange aggregate and raises <see cref="ManagementOfChangeCreatedEvent"/>.</summary>
    public static ManagementOfChange Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string changeTitle = "",
        string changeType = "Process",
        string justification = "",
        string riskLevel = "Low",
        UserId requestedByUserId = default,
        UserId? approvedByUserId = null,
        string approvalStatus = "Pending",
        UtcTimestamp? implementationAtUtc = null,
        UtcTimestamp? reviewDueAtUtc = null)
    {
        var agg = new ManagementOfChange
        {
            ManagementOfChangeId  = ManagementOfChangeId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = ManagementOfChangeStatus.Created,
            ChangeTitle           = (changeTitle ?? string.Empty).Trim(),
            ChangeType            = (changeType ?? "Process").Trim(),
            Justification         = (justification ?? string.Empty).Trim(),
            RiskLevel             = (riskLevel ?? "Low").Trim(),
            RequestedByUserId     = requestedByUserId,
            ApprovedByUserId      = approvedByUserId,
            ApprovalStatus        = (approvalStatus ?? "Pending").Trim(),
            ImplementationAtUtc   = implementationAtUtc,
            ReviewDueAtUtc        = reviewDueAtUtc,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new ManagementOfChangeCreatedEvent(
            EventId:       new ManagementOfChangeEventId(Uuid7.New()),
            ManagementOfChangeId:      agg.ManagementOfChangeId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IManagementOfChangeDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ManagementOfChangeStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for ManagementOfChange {ManagementOfChangeId}.");
    }
}
