// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Procedures
//  Layer:          Domain
//  File:           Procedures.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  procedure-document record shape. NOTE: no dedicated Procedures
//                  field-level spec was found in project_plantsafety_domains.md
//                  (v4.0 lists only "Policy" as Module 13); project_plantsafety_overview.md
//                  Section 6 confirms "Policies & Procedures" (4.47) as a distinct
//                  sub-module. Properties below mirror the sibling Policy aggregate's
//                  document-lifecycle shape (industry-standard SOP/procedure fields)
//                  since no deeper manual exists. Flagged as partially-unsourced.
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Procedures bounded context. Tracks a single
//                  standard operating procedure (SOP) document: its number, category,
//                  revision, owner, and review schedule.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Procedures.Aggregates;

/// <summary>Aggregate root for the Procedures bounded context.</summary>
public sealed partial class Procedures
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Procedures aggregate.</summary>
    public ProceduresId ProceduresId { get; private set; }

    /// <summary>Tenant that owns this procedure record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this procedure is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ProceduresStatus Status { get; private set; }

    // ---- Procedure-specific business properties ------------------------------

    /// <summary>Human-readable procedure title (e.g., "Confined Space Entry SOP").</summary>
    public string ProcedureTitle { get; private set; } = string.Empty;

    /// <summary>Procedure document number/reference.</summary>
    public string ProcedureNumber { get; private set; } = string.Empty;

    /// <summary>Procedure category (e.g., LOTO, HotWork, ConfinedSpace, EmergencyResponse).</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Monotonically increasing revision number, starting at 1.</summary>
    public int RevisionNumber { get; private set; } = 1;

    /// <summary>Identity of the accountable procedure owner.</summary>
    public UserId OwnerId { get; private set; }

    /// <summary>UTC timestamp the procedure becomes effective.</summary>
    public UtcTimestamp? EffectiveAtUtc { get; private set; }

    /// <summary>UTC timestamp the procedure is next due for review.</summary>
    public UtcTimestamp? ReviewDueAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IProceduresDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IProceduresDomainEvent> DomainEvents
        => new ReadOnlyCollection<IProceduresDomainEvent>(_domainEvents);

    // For rehydration only.
    private Procedures() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Procedures aggregate and raises <see cref="ProceduresCreatedEvent"/>.</summary>
    public static Procedures Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string procedureTitle = "",
        string procedureNumber = "",
        string category = "",
        int revisionNumber = 1,
        UserId ownerId = default,
        UtcTimestamp? effectiveAtUtc = null,
        UtcTimestamp? reviewDueAtUtc = null)
    {
        var agg = new Procedures
        {
            ProceduresId       = ProceduresId.New(),
            TenantId           = tenantId,
            SiteId             = siteId,
            Status             = ProceduresStatus.Created,
            ProcedureTitle     = (procedureTitle ?? string.Empty).Trim(),
            ProcedureNumber    = (procedureNumber ?? string.Empty).Trim(),
            Category           = (category ?? string.Empty).Trim(),
            RevisionNumber     = revisionNumber < 1 ? 1 : revisionNumber,
            OwnerId            = ownerId,
            EffectiveAtUtc     = effectiveAtUtc,
            ReviewDueAtUtc     = reviewDueAtUtc,
            CreatedAtUtc       = createdAtUtc,
            CreatedBy          = createdBy,
        };

        agg.AddDomainEvent(new ProceduresCreatedEvent(
            EventId:       new ProceduresEventId(Uuid7.New()),
            ProceduresId:      agg.ProceduresId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IProceduresDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ProceduresStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Procedures {ProceduresId}.");
    }
}
