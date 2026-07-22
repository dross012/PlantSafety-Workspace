// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.RoutineSafetyAuditsIncidentReporting
//  Layer:          Domain
//  File:           RoutineSafetyAuditsIncidentReportingCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real routine safety audit/incident reporting
//                  lifecycle: scheduling, conduct (clean/findings outcome), corrective action, and
//                  closure.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Events;

/// <summary>Raised when a new audit/walkthrough/report cycle is scheduled.</summary>
public sealed record RoutineSafetyAuditScheduledEvent(
    RoutineSafetyAuditsIncidentReportingEventId EventId,
    RoutineSafetyAuditsIncidentReportingId RoutineSafetyAuditsIncidentReportingId,
    SafetyReportingCategory ReportingCategory,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRoutineSafetyAuditsIncidentReportingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RoutineSafetyAuditScheduled";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the audit is conducted with no findings.</summary>
public sealed record RoutineSafetyAuditConductedEvent(
    RoutineSafetyAuditsIncidentReportingEventId EventId,
    RoutineSafetyAuditsIncidentReportingId RoutineSafetyAuditsIncidentReportingId,
    decimal ComplianceScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRoutineSafetyAuditsIncidentReportingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RoutineSafetyAuditConducted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when findings are logged during the audit.</summary>
public sealed record RoutineSafetyAuditFindingsLoggedEvent(
    RoutineSafetyAuditsIncidentReportingEventId EventId,
    RoutineSafetyAuditsIncidentReportingId RoutineSafetyAuditsIncidentReportingId,
    int FindingCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRoutineSafetyAuditsIncidentReportingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RoutineSafetyAuditFindingsLogged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when corrective action begins to remediate logged findings.</summary>
public sealed record RoutineSafetyAuditCorrectiveActionStartedEvent(
    RoutineSafetyAuditsIncidentReportingEventId EventId,
    RoutineSafetyAuditsIncidentReportingId RoutineSafetyAuditsIncidentReportingId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRoutineSafetyAuditsIncidentReportingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RoutineSafetyAuditCorrectiveActionStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the audit cycle is closed out and the next cycle scheduled.</summary>
public sealed record RoutineSafetyAuditClosedEvent(
    RoutineSafetyAuditsIncidentReportingEventId EventId,
    RoutineSafetyAuditsIncidentReportingId RoutineSafetyAuditsIncidentReportingId,
    UtcTimestamp NextDueAtUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IRoutineSafetyAuditsIncidentReportingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "RoutineSafetyAuditClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
