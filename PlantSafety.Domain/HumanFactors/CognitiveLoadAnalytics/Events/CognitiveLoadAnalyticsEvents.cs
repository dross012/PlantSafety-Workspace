// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Domain
//  File:           CognitiveLoadAnalyticsEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the CognitiveLoadLedger and CognitiveRiskPrediction
//                  aggregates: reading capture, overload detection, trend recalculation, and
//                  predictive risk generation/acknowledgement. CognitiveOverloadDetectedEvent and
//                  CognitiveRiskPredictionGeneratedEvent (when Elevated/Critical) are the risk hooks
//                  consumed by RiskManagement and correlated against AccidentManagement root-cause
//                  data. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Events;

/// <summary>Raised when a cognitive-load reading is recorded on a ledger.</summary>
public sealed record CognitiveLoadReadingRecordedEvent(
    CognitiveLoadAnalyticsEventId EventId,
    CognitiveLoadLedgerId CognitiveLoadLedgerId,
    CognitiveLoadReadingId CognitiveLoadReadingId,
    Guid WorkerUserId,
    decimal CognitiveLoadScore,
    CognitiveWorkloadLevel WorkloadLevel,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICognitiveLoadAnalyticsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CognitiveLoadReadingRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>
/// Raised when a recorded cognitive-load reading meets or exceeds the overload threshold
/// (<see cref="Policies.CognitiveOverloadPolicy"/>). Risk hook consumed by RiskManagement.
/// </summary>
public sealed record CognitiveOverloadDetectedEvent(
    CognitiveLoadAnalyticsEventId EventId,
    CognitiveLoadLedgerId CognitiveLoadLedgerId,
    CognitiveLoadReadingId CognitiveLoadReadingId,
    Guid WorkerUserId,
    decimal CognitiveLoadScore,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICognitiveLoadAnalyticsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CognitiveOverloadDetected";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a shift's workload trend is (re)calculated from its recorded readings.</summary>
public sealed record WorkloadTrendRecalculatedEvent(
    CognitiveLoadAnalyticsEventId EventId,
    CognitiveLoadLedgerId CognitiveLoadLedgerId,
    WorkloadTrendRecordId WorkloadTrendRecordId,
    decimal MeanCognitiveLoadScore,
    int OverloadEventCount,
    int ReadingCount,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICognitiveLoadAnalyticsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkloadTrendRecalculated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>
/// Raised when a new cognitive-risk prediction is generated for a worker/shift. Elevated/Critical
/// predictions are the risk hook consumed by RiskManagement and correlated against
/// AccidentManagement root-cause data; also feeds the predictive model retraining schedule.
/// </summary>
public sealed record CognitiveRiskPredictionGeneratedEvent(
    CognitiveLoadAnalyticsEventId EventId,
    CognitiveRiskPredictionId CognitiveRiskPredictionId,
    Guid WorkerUserId,
    CognitiveLoadLedgerId SourceLedgerId,
    CognitiveRiskLevel PredictedRiskLevel,
    decimal PredictionScore,
    string ModelVersion,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICognitiveLoadAnalyticsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CognitiveRiskPredictionGenerated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>
/// Raised when a cognitive-risk alert is acknowledged by a responder. Backs the acknowledgement
/// response-time KPI.
/// </summary>
public sealed record CognitiveRiskAlertAcknowledgedEvent(
    CognitiveLoadAnalyticsEventId EventId,
    CognitiveRiskPredictionId CognitiveRiskPredictionId,
    Guid AcknowledgedByUserId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ICognitiveLoadAnalyticsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CognitiveRiskAlertAcknowledged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
