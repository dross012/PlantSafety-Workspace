// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PPEManagement
//  Layer:          Domain
//  File:           PPEManagement.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  PPE allocation/tracking record shape grounded in
//                  project_plantsafety_domains.md Module 14 (PPE: "PPE allocation,
//                  compliance, and tracking"). Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the PPEManagement bounded context. Tracks a
//                  single PPE issuance record: the worker, item, issue/return dates,
//                  and inspection/compliance state.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.PPEManagement.Aggregates;

/// <summary>Aggregate root for the PPEManagement bounded context.</summary>
public sealed partial class PPEManagement
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this PPEManagement aggregate.</summary>
    public PPEManagementId PPEManagementId { get; private set; }

    /// <summary>Tenant that owns this PPE record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this PPE record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public PPEManagementStatus Status { get; private set; }

    // ---- PPE-specific business properties -------------------------------------

    /// <summary>Worker this PPE record is issued to.</summary>
    public UserId WorkerId { get; private set; }

    /// <summary>PPE type classification (e.g., HeadProtection, RespiratoryProtection, FallProtection).</summary>
    public string PpeType { get; private set; } = string.Empty;

    /// <summary>Specific PPE item description/model.</summary>
    public string PpeItem { get; private set; } = string.Empty;

    /// <summary>Compliance status: Compliant, Overdue, Expired, NonCompliant.</summary>
    public string ComplianceStatus { get; private set; } = "Compliant";

    /// <summary>UTC timestamp the PPE was issued.</summary>
    public UtcTimestamp? IssuedAtUtc { get; private set; }

    /// <summary>UTC timestamp the PPE was returned, if applicable.</summary>
    public UtcTimestamp? ReturnedAtUtc { get; private set; }

    /// <summary>UTC timestamp the PPE is next due for inspection.</summary>
    public UtcTimestamp? InspectionDueAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IPPEManagementDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IPPEManagementDomainEvent> DomainEvents
        => new ReadOnlyCollection<IPPEManagementDomainEvent>(_domainEvents);

    // For rehydration only.
    private PPEManagement() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new PPEManagement aggregate and raises <see cref="PPEManagementCreatedEvent"/>.</summary>
    public static PPEManagement Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        UserId workerId = default,
        string ppeType = "",
        string ppeItem = "",
        string complianceStatus = "Compliant",
        UtcTimestamp? issuedAtUtc = null,
        UtcTimestamp? inspectionDueAtUtc = null)
    {
        var agg = new PPEManagement
        {
            PPEManagementId       = PPEManagementId.New(),
            TenantId              = tenantId,
            SiteId                = siteId,
            Status                = PPEManagementStatus.Created,
            WorkerId              = workerId,
            PpeType               = (ppeType ?? string.Empty).Trim(),
            PpeItem               = (ppeItem ?? string.Empty).Trim(),
            ComplianceStatus      = (complianceStatus ?? "Compliant").Trim(),
            IssuedAtUtc           = issuedAtUtc ?? createdAtUtc,
            InspectionDueAtUtc    = inspectionDueAtUtc,
            CreatedAtUtc          = createdAtUtc,
            CreatedBy             = createdBy,
        };

        agg.AddDomainEvent(new PPEManagementCreatedEvent(
            EventId:       new PPEManagementEventId(Uuid7.New()),
            PPEManagementId:      agg.PPEManagementId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IPPEManagementDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params PPEManagementStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for PPEManagement {PPEManagementId}.");
    }
}
