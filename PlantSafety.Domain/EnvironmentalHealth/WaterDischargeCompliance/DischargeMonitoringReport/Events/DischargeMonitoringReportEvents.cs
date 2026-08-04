// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the DischargeMonitoringReport lifecycle:
//                  preparation, submission, acceptance, rejection. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Events;

/// <summary>Raised when a new DMR is prepared in Draft status.</summary>
public sealed record DischargeMonitoringReportPreparedEvent(
    DischargeMonitoringReportEventId EventId,
    DischargeMonitoringReportId DischargeMonitoringReportId,
    Guid DischargePermitId,
    UtcTimestamp ReportingPeriodStartUtc,
    UtcTimestamp ReportingPeriodEndUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargeMonitoringReportDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargeMonitoringReportPrepared";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a DMR is submitted to the regulator (tied to Compliance.RegulatoryCalendar
/// due-date tracking).</summary>
public sealed record DischargeMonitoringReportSubmittedEvent(
    DischargeMonitoringReportEventId EventId,
    DischargeMonitoringReportId DischargeMonitoringReportId,
    Guid SubmittedByUserId,
    bool WasOnTime,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargeMonitoringReportDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargeMonitoringReportSubmitted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a submitted DMR is accepted by the regulator.</summary>
public sealed record DischargeMonitoringReportAcceptedEvent(
    DischargeMonitoringReportEventId EventId,
    DischargeMonitoringReportId DischargeMonitoringReportId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargeMonitoringReportDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargeMonitoringReportAccepted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a submitted DMR is rejected by the regulator.</summary>
public sealed record DischargeMonitoringReportRejectedEvent(
    DischargeMonitoringReportEventId EventId,
    DischargeMonitoringReportId DischargeMonitoringReportId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IDischargeMonitoringReportDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "DischargeMonitoringReportRejected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}