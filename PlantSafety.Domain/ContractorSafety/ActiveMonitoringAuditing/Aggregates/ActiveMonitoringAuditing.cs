// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditing.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Aggregate root for a single active monitoring/auditing observation of a contractor
//                  once work begins: site visits, PPE compliance verification, equipment/LOTO
//                  inspection, and permit-condition adherence. Models the real lifecycle: scheduling,
//                  observation, unsafe-condition flagging, follow-up, and resolution.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Aggregates;

/// <summary>Aggregate root for a single active monitoring/auditing observation of a contractor.</summary>
public sealed partial class ActiveMonitoringAuditing
{
    // ---- Identity ----------------------------------------------------------

    /// <summary>Strong-typed identity of this ActiveMonitoringAuditing aggregate.</summary>
    public ActiveMonitoringAuditingId ActiveMonitoringAuditingId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public ActiveMonitoringAuditingStatus Status { get; private set; }

    // ---- Observation identity ---------------------------------------------------

    /// <summary>Human-readable title of this monitoring/audit visit.</summary>
    public string MonitoringTitle { get; private set; } = string.Empty;

    /// <summary>Observation category.</summary>
    public ContractorMonitoringCategory Category { get; private set; }

    /// <summary>Work location observed.</summary>
    public string WorkLocation { get; private set; } = string.Empty;

    /// <summary>Identity of the employee who performed the observation.</summary>
    public UserId MonitoredByUserId { get; private set; }

    /// <summary>UTC timestamp the observation was recorded, if any.</summary>
    public UtcTimestamp? ObservedAtUtc { get; private set; }

    /// <summary>Count of unsafe conditions/behaviors flagged during the observation.</summary>
    public int UnsafeConditionCount { get; private set; }

    /// <summary>Compliance score (0-100) from the observation.</summary>
    public decimal? ComplianceScore { get; private set; }

    /// <summary>Narrative findings documented during the observation.</summary>
    public string? FindingNotes { get; private set; }

    /// <summary>UTC timestamp a follow-up visit is due, if scheduled.</summary>
    public UtcTimestamp? FollowUpAtUtc { get; private set; }

    // ---- Audit -----------------------------------------------------------------

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    // ---- Domain Events -----------------------------------------------------

    private readonly List<IActiveMonitoringAuditingDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IActiveMonitoringAuditingDomainEvent> DomainEvents
        => new ReadOnlyCollection<IActiveMonitoringAuditingDomainEvent>(_domainEvents);

    // For rehydration only.
    private ActiveMonitoringAuditing() { }

    // ---- Factory: Create -----------------------------------------------------------

    /// <summary>Schedules a new monitoring/audit visit and raises <see cref="ActiveMonitoringAuditingScheduledEvent"/>.</summary>
    public static ActiveMonitoringAuditing Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        string monitoringTitle = "",
        ContractorMonitoringCategory category = ContractorMonitoringCategory.SiteVisit,
        string workLocation = "",
        UserId monitoredByUserId = default)
    {
        var agg = new ActiveMonitoringAuditing
        {
            ActiveMonitoringAuditingId = ActiveMonitoringAuditingId.New(),
            TenantId           = tenantId,
            SiteId             = siteId,
            Status             = ActiveMonitoringAuditingStatus.Scheduled,
            MonitoringTitle    = (monitoringTitle ?? string.Empty).Trim(),
            Category           = category,
            WorkLocation       = (workLocation ?? string.Empty).Trim(),
            MonitoredByUserId  = monitoredByUserId,
            CreatedAtUtc       = createdAtUtc,
            CreatedBy          = createdBy,
        };

        agg.AddDomainEvent(new ActiveMonitoringAuditingScheduledEvent(
            ActiveMonitoringAuditingEventId.New(), agg.ActiveMonitoringAuditingId, category,
            createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    // ---- Behavior: RecordObservation -----------------------------------------------

    /// <summary>Records the observation outcome; flags an unsafe condition if any were found.</summary>
    public void RecordObservation(int unsafeConditionCount, decimal complianceScore, string? findingNotes, UtcTimestamp observedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ActiveMonitoringAuditingStatus.Scheduled, ActiveMonitoringAuditingStatus.FollowUpScheduled);

        if (unsafeConditionCount < 0)
            throw new ArgumentOutOfRangeException(nameof(unsafeConditionCount), "Unsafe condition count cannot be negative.");
        if (complianceScore is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(complianceScore), "Compliance score must be between 0 and 100.");

        UnsafeConditionCount = unsafeConditionCount;
        ComplianceScore      = complianceScore;
        FindingNotes         = findingNotes;
        ObservedAtUtc        = observedAtUtc;

        if (unsafeConditionCount > 0)
        {
            Status = ActiveMonitoringAuditingStatus.UnsafeConditionFlagged;
            AddDomainEvent(new ActiveMonitoringAuditingUnsafeConditionFlaggedEvent(
                ActiveMonitoringAuditingEventId.New(), ActiveMonitoringAuditingId, unsafeConditionCount, findingNotes ?? string.Empty,
                observedAtUtc, actor, correlationId, causationId));
        }
        else
        {
            Status = ActiveMonitoringAuditingStatus.ObservationRecorded;
            AddDomainEvent(new ActiveMonitoringAuditingObservationRecordedEvent(
                ActiveMonitoringAuditingEventId.New(), ActiveMonitoringAuditingId, complianceScore,
                observedAtUtc, actor, correlationId, causationId));
        }
    }

    // ---- Behavior: ScheduleFollowUp -----------------------------------------------

    /// <summary>Schedules a follow-up visit to verify correction of a flagged unsafe condition.</summary>
    public void ScheduleFollowUp(UtcTimestamp dueAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ActiveMonitoringAuditingStatus.UnsafeConditionFlagged);

        FollowUpAtUtc = dueAtUtc;
        Status = ActiveMonitoringAuditingStatus.FollowUpScheduled;

        AddDomainEvent(new ActiveMonitoringAuditingFollowUpScheduledEvent(
            ActiveMonitoringAuditingEventId.New(), ActiveMonitoringAuditingId, dueAtUtc, actor, correlationId, causationId));
    }

    // ---- Behavior: Resolve -----------------------------------------------------------

    /// <summary>Resolves the flagged condition once the follow-up visit confirms correction.</summary>
    public void Resolve(UtcTimestamp resolvedAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        EnsureStatusIsOneOf(ActiveMonitoringAuditingStatus.FollowUpScheduled);

        Status = ActiveMonitoringAuditingStatus.Resolved;

        AddDomainEvent(new ActiveMonitoringAuditingResolvedEvent(
            ActiveMonitoringAuditingEventId.New(), ActiveMonitoringAuditingId, resolvedAtUtc, actor, correlationId, causationId));
    }

    // ---- Helpers -----------------------------------------------------------

    private void AddDomainEvent(IActiveMonitoringAuditingDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatusIsOneOf(params ActiveMonitoringAuditingStatus[] allowed)
    {
        foreach (var s in allowed)
            if (Status == s) return;

        throw new InvalidOperationException(
            $"Invalid lifecycle transition from '{Status}' for ActiveMonitoringAuditing {ActiveMonitoringAuditingId}.");
    }
}
