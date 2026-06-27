// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.PredictiveAnalytics
//  Layer:          Domain
//  File:           PredictiveAnalyticsCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.PredictiveAnalytics.Events;

/// <summary>Raised when a new <see cref="PredictiveAnalytics"/> aggregate is created.</summary>
public sealed record PredictiveAnalyticsCreatedEvent(
    PredictiveAnalyticsEventId EventId,
    PredictiveAnalyticsId      PredictiveAnalyticsId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IPredictiveAnalyticsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "PredictiveAnalyticsCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
