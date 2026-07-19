// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.Schedules
//  Layer:          Domain
//  File:           Schedules.cs
//  Created:        2026-06-17
//  Updated:        2026-07-18 -- extended from identity-only scaffold with the full
//                  scheduled-task record shape grounded in project_plantsafety_domains.md
//                  Module 16 (Schedules: "Recurring task scheduling and SLA
//                  governance"). Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for the Schedules bounded context. Tracks a single
//                  recurring/scheduled task: its recurrence pattern, due date,
//                  assignment, and SLA window.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.Schedules.Aggregates;

/// <summary>Aggregate root for the Schedules bounded context.</summary>
public sealed partial class Schedules
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this Schedules aggregate.</summary>
    public SchedulesId SchedulesId { get; private set; }

    /// <summary>Tenant that owns this schedule record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this scheduled task is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public SchedulesStatus Status { get; private set; }

    // ---- Schedule-specific business properties ---------------------------------

    /// <summary>Human-readable task name.</summary>
    public string TaskName { get; private set; } = string.Empty;

    /// <summary>Recurrence pattern (e.g., Daily, Weekly, Monthly, Quarterly, OneTime).</summary>
    public string RecurrencePattern { get; private set; } = "OneTime";

    /// <summary>Identity of the user assigned to complete this task.</summary>
    public UserId AssignedToUserId { get; private set; }

    /// <summary>UTC timestamp this task instance is due.</summary>
    public UtcTimestamp? DueAtUtc { get; private set; }

    /// <summary>SLA window, in minutes, allowed for task completion once due.</summary>
    public int SlaWindowMinutes { get; private set; } = 1440;

    /// <summary>UTC timestamp this task instance was completed.</summary>
    public UtcTimestamp? CompletedAtUtc { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<ISchedulesDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<ISchedulesDomainEvent> DomainEvents
        => new ReadOnlyCollection<ISchedulesDomainEvent>(_domainEvents);

    // For rehydration only.
    private Schedules() { }

    // ---- Factory -----------------------------------------------------------

    /// <summary>Creates a new Schedules aggregate and raises <see cref="SchedulesCreatedEvent"/>.</summary>
    public static Schedules Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string taskName = "",
        string recurrencePattern = "OneTime",
        UserId assignedToUserId = default,
        UtcTimestamp? dueAtUtc = null,
        int slaWindowMinutes = 1440)
    {
        var agg = new Schedules
        {
            SchedulesId          = SchedulesId.New(),
            TenantId             = tenantId,
            SiteId               = siteId,
            Status               = SchedulesStatus.Created,
            TaskName             = (taskName ?? string.Empty).Trim(),
            RecurrencePattern    = (recurrencePattern ?? "OneTime").Trim(),
            AssignedToUserId     = assignedToUserId,
            DueAtUtc             = dueAtUtc,
            SlaWindowMinutes     = slaWindowMinutes < 0 ? 0 : slaWindowMinutes,
            CreatedAtUtc         = createdAtUtc,
            CreatedBy            = createdBy,
        };

        agg.AddDomainEvent(new SchedulesCreatedEvent(
            EventId:       new SchedulesEventId(Uuid7.New()),
            SchedulesId:      agg.SchedulesId,
            CreatedAtUtc:  createdAtUtc,
            Actor:         actor,
            CorrelationId: correlationId,
            CausationId:   causationId));

        return agg;
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(ISchedulesDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params SchedulesStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for Schedules {SchedulesId}.");
    }
}
