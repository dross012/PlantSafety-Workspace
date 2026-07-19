// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Checklists
//  Layer:          Domain
//  File:           Checklists.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  checklist-execution record shape grounded in
//                  project_plantsafety_domains.md Module 4 (Checklist: "Structured
//                  inspections, verification, and procedural compliance").
//                  Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Checklists bounded context. Tracks a single
//                  checklist execution: its type, item completion counters, exception
//                  count, and completion authorship.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Checklists.Aggregates;

/// <summary>Aggregate root for the Checklists bounded context.</summary>
public sealed partial class Checklists
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Checklists aggregate.</summary>
    public ChecklistsId ChecklistsId { get; private set; }

    /// <summary>Tenant that owns this checklist record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this checklist is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ChecklistsStatus Status { get; private set; }

    // ---- Checklist-specific business properties ------------------------------

    /// <summary>Human-readable checklist title.</summary>
    public string ChecklistTitle { get; private set; } = string.Empty;

    /// <summary>Checklist classification (e.g., PreShift, EquipmentInspection, SafetyWalkdown).</summary>
    public string ChecklistType { get; private set; } = string.Empty;

    /// <summary>Physical location the checklist was executed at.</summary>
    public string Location { get; private set; } = string.Empty;

    /// <summary>Total number of items on this checklist.</summary>
    public int TotalItems { get; private set; }

    /// <summary>Number of items completed so far.</summary>
    public int CompletedItems { get; private set; }

    /// <summary>Count of exceptions/failures raised during execution.</summary>
    public int ExceptionCount { get; private set; }

    /// <summary>Identity of the user who completed the checklist, if completed.</summary>
    public UserId? CompletedByUserId { get; private set; }

    /// <summary>UTC timestamp the checklist was completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IChecklistsDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IChecklistsDomainEvent> DomainEvents
        => new ReadOnlyCollection<IChecklistsDomainEvent>(_domainEvents);

    // For rehydration only.
    private Checklists() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Checklists aggregate and raises <see cref="ChecklistsCreatedEvent"/>.</summary>
    public static Checklists Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string checklistTitle = "",
        string checklistType = "",
        string location = "",
        int totalItems = 0,
        int completedItems = 0,
        int exceptionCount = 0)
    {
        var agg = new Checklists
        {
            ChecklistsId    = ChecklistsId.New(),
            TenantId        = tenantId,
            SiteId          = siteId,
            Status          = ChecklistsStatus.Created,
            ChecklistTitle  = (checklistTitle ?? string.Empty).Trim(),
            ChecklistType   = (checklistType ?? string.Empty).Trim(),
            Location        = (location ?? string.Empty).Trim(),
            TotalItems      = totalItems < 0 ? 0 : totalItems,
            CompletedItems  = completedItems < 0 ? 0 : completedItems,
            ExceptionCount  = exceptionCount < 0 ? 0 : exceptionCount,
            CreatedAtUtc    = createdAtUtc,
            CreatedBy       = createdBy,
        };

        agg.AddDomainEvent(new ChecklistsCreatedEvent(
            EventId:       new ChecklistsEventId(Uuid7.New()),
            ChecklistsId:      agg.ChecklistsId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IChecklistsDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ChecklistsStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Checklists {ChecklistsId}.");
    }
}
