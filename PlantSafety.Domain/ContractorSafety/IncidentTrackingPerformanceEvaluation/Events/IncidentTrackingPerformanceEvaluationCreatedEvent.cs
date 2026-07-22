// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.IncidentTrackingPerformanceEvaluation
//  Layer:          Domain
//  File:           IncidentTrackingPerformanceEvaluationCreatedEvent.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the real incident tracking/performance-evaluation
//                  lifecycle: reporting, investigation, CAPA documentation, and eligibility
//                  determination.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.IncidentTrackingPerformanceEvaluation.Events;

/// <summary>Raised when a contractor incident/near-miss/unsafe condition is reported.</summary>
public sealed record ContractorIncidentReportedEvent(
    IncidentTrackingPerformanceEvaluationEventId EventId,
    IncidentTrackingPerformanceEvaluationId IncidentTrackingPerformanceEvaluationId,
    ContractorIncidentCategory IncidentCategory,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentTrackingPerformanceEvaluationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ContractorIncidentReported";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the investigation and root-cause analysis begins.</summary>
public sealed record ContractorIncidentInvestigationStartedEvent(
    IncidentTrackingPerformanceEvaluationEventId EventId,
    IncidentTrackingPerformanceEvaluationId IncidentTrackingPerformanceEvaluationId,
    UserId EvaluatedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentTrackingPerformanceEvaluationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ContractorIncidentInvestigationStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when CAPA items are documented following the investigation.</summary>
public sealed record ContractorIncidentCapaDocumentedEvent(
    IncidentTrackingPerformanceEvaluationEventId EventId,
    IncidentTrackingPerformanceEvaluationId IncidentTrackingPerformanceEvaluationId,
    int CapaCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentTrackingPerformanceEvaluationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ContractorIncidentCapaDocumented";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the contractor is approved for future work.</summary>
public sealed record ContractorEligibilityApprovedEvent(
    IncidentTrackingPerformanceEvaluationEventId EventId,
    IncidentTrackingPerformanceEvaluationId IncidentTrackingPerformanceEvaluationId,
    decimal PerformanceScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentTrackingPerformanceEvaluationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ContractorEligibilityApproved";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when the contractor's eligibility for future work is restricted.</summary>
public sealed record ContractorEligibilityRestrictedEvent(
    IncidentTrackingPerformanceEvaluationEventId EventId,
    IncidentTrackingPerformanceEvaluationId IncidentTrackingPerformanceEvaluationId,
    decimal PerformanceScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IIncidentTrackingPerformanceEvaluationDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ContractorEligibilityRestricted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
