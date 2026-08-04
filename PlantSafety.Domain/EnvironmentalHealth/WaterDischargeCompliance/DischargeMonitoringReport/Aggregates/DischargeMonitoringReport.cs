// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReport.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Aggregate root for a periodic Discharge Monitoring Report (DMR) submitted to the
//                  regulator for a DischargePermit, tied to Compliance.RegulatoryCalendar due-date
//                  tracking. Regulatory submissions are retained 5+ years (RetentionExpiresAtUtc set
//                  on submission). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Aggregates;

/// <summary>Aggregate root for a periodic Discharge Monitoring Report (DMR).</summary>
public sealed partial class DischargeMonitoringReport
{
    /// <summary>Minimum regulatory-submission retention period, in years.</summary>
    public const int MinimumRetentionYears = 5;

    /// <summary>Strong-typed identity of this DischargeMonitoringReport aggregate.</summary>
    public DischargeMonitoringReportId DischargeMonitoringReportId { get; private set; }

    /// <summary>Tenant that owns this record.</summary>
    public Guid TenantId { get; private set; }

    /// <summary>Site this record is scoped to. Null means enterprise-level.</summary>
    public Guid? SiteId { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public DischargeMonitoringReportStatus Status { get; private set; }

    /// <summary>Identity of the owning DischargePermit aggregate.</summary>
    public Guid DischargePermitId { get; private set; }

    /// <summary>UTC start of the reporting period this DMR covers.</summary>
    public UtcTimestamp ReportingPeriodStartUtc { get; private set; }

    /// <summary>UTC end of the reporting period this DMR covers.</summary>
    public UtcTimestamp ReportingPeriodEndUtc { get; private set; }

    /// <summary>UTC instant the DMR is due to the regulator (Compliance.RegulatoryCalendar hook).</summary>
    public UtcTimestamp DueAtUtc { get; private set; }

    /// <summary>UTC instant the DMR was submitted, if any.</summary>
    public UtcTimestamp? SubmittedAtUtc { get; private set; }

    /// <summary>Identity of the user who submitted the DMR, if any.</summary>
    public UserId? SubmittedByUserId { get; private set; }

    /// <summary>Count of samples within limit for this reporting period (feeds the "percent samples
    /// within limit" KPI).</summary>
    public int SamplesWithinLimitCount { get; private set; }

    /// <summary>Total count of samples taken for this reporting period.</summary>
    public int TotalSamplesCount { get; private set; }

    /// <summary>UTC instant this record becomes eligible for archival (5+ year regulatory retention).</summary>
    public UtcTimestamp? RetentionExpiresAtUtc { get; private set; }

    /// <summary>Reason recorded when the regulator rejects the DMR.</summary>
    public string? RejectionReason { get; private set; }

    /// <summary>UTC creation timestamp.</summary>
    public UtcTimestamp CreatedAtUtc { get; private set; }

    /// <summary>Identity of the user who created this record (CRUD authorship).</summary>
    public UserId CreatedBy { get; private set; }

    private readonly List<IDischargeMonitoringReportDomainEvent> _domainEvents = [];

    /// <summary>Domain events raised during this unit of work.</summary>
    public IReadOnlyCollection<IDischargeMonitoringReportDomainEvent> DomainEvents
        => new ReadOnlyCollection<IDischargeMonitoringReportDomainEvent>(_domainEvents);

    private DischargeMonitoringReport() { }

    /// <summary>Prepares a new DMR in Draft status and raises <see cref="DischargeMonitoringReportPreparedEvent"/>.</summary>
    public static DischargeMonitoringReport Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId = default,
        Guid? siteId = null,
        Guid dischargePermitId = default,
        UtcTimestamp reportingPeriodStartUtc = default,
        UtcTimestamp reportingPeriodEndUtc = default,
        UtcTimestamp dueAtUtc = default)
    {
        var agg = new DischargeMonitoringReport
        {
            DischargeMonitoringReportId = DischargeMonitoringReportId.New(),
            TenantId = tenantId,
            SiteId = siteId,
            Status = DischargeMonitoringReportStatus.Draft,
            DischargePermitId = dischargePermitId,
            ReportingPeriodStartUtc = reportingPeriodStartUtc,
            ReportingPeriodEndUtc = reportingPeriodEndUtc,
            DueAtUtc = dueAtUtc,
            SamplesWithinLimitCount = 0,
            TotalSamplesCount = 0,
            CreatedAtUtc = createdAtUtc,
            CreatedBy = createdBy,
        };

        agg.AddDomainEvent(new DischargeMonitoringReportPreparedEvent(
            DischargeMonitoringReportEventId.New(), agg.DischargeMonitoringReportId, dischargePermitId,
            reportingPeriodStartUtc, reportingPeriodEndUtc, createdAtUtc, actor, correlationId, causationId));

        return agg;
    }

    /// <summary>Submits the DMR to the regulator. Sets status to Submitted (on-time) or Late, and
    /// starts the 5+ year regulatory retention clock.</summary>
    public void Submit(
        UserId submittedByUserId, UtcTimestamp submittedAtUtc,
        int samplesWithinLimitCount, int totalSamplesCount,
        PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargeMonitoringReportLifecycleRules.CanSubmit(Status))
            throw new DischargeMonitoringReportInvalidStatusTransitionException(DischargeMonitoringReportId, Status.ToString(), DischargeMonitoringReportStatus.Submitted.ToString(), correlationId, causationId);

        var wasOnTime = submittedAtUtc.Value <= DueAtUtc.Value;

        SubmittedAtUtc = submittedAtUtc;
        SubmittedByUserId = submittedByUserId;
        SamplesWithinLimitCount = samplesWithinLimitCount;
        TotalSamplesCount = totalSamplesCount;
        RetentionExpiresAtUtc = UtcTimestamp.From(submittedAtUtc.Value.AddYears(MinimumRetentionYears));
        Status = wasOnTime ? DischargeMonitoringReportStatus.Submitted : DischargeMonitoringReportStatus.Late;

        AddDomainEvent(new DischargeMonitoringReportSubmittedEvent(
            DischargeMonitoringReportEventId.New(), DischargeMonitoringReportId, submittedByUserId.Value, wasOnTime, submittedAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Records regulator acceptance of the DMR.</summary>
    public void Accept(UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargeMonitoringReportLifecycleRules.CanAccept(Status))
            throw new DischargeMonitoringReportInvalidStatusTransitionException(DischargeMonitoringReportId, Status.ToString(), DischargeMonitoringReportStatus.Accepted.ToString(), correlationId, causationId);

        Status = DischargeMonitoringReportStatus.Accepted;

        AddDomainEvent(new DischargeMonitoringReportAcceptedEvent(
            DischargeMonitoringReportEventId.New(), DischargeMonitoringReportId, occurredAtUtc, actor, correlationId, causationId));
    }

    /// <summary>Records regulator rejection of the DMR.</summary>
    public void Reject(string reason, UtcTimestamp occurredAtUtc, PerformedByActor actor, CorrelationId correlationId, CausationId causationId)
    {
        if (!DischargeMonitoringReportLifecycleRules.CanReject(Status))
            throw new DischargeMonitoringReportInvalidStatusTransitionException(DischargeMonitoringReportId, Status.ToString(), DischargeMonitoringReportStatus.Rejected.ToString(), correlationId, causationId);

        Status = DischargeMonitoringReportStatus.Rejected;
        RejectionReason = reason;

        AddDomainEvent(new DischargeMonitoringReportRejectedEvent(
            DischargeMonitoringReportEventId.New(), DischargeMonitoringReportId, reason, occurredAtUtc, actor, correlationId, causationId));
    }

    private void AddDomainEvent(IDischargeMonitoringReportDomainEvent evt) => _domainEvents.Add(evt);

    /// <summary>Clears the in-memory event list after dispatch.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}