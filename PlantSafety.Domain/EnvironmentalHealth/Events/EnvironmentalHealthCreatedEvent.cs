// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           EnvironmentalHealthCreatedEvent.cs
//  Created:        2026-06-17
//  Updated:        2026-07-15 — added TenantId/MetricType/MeasurementValue/Status payload
//                  fields (additive) to match the extended EnvironmentalHealth aggregate shape.
//  Updated:        2026-08-02 — Phase 4 full build-out: renamed MetricType -> MonitoringType and
//                  MeasurementValue -> MeasuredValue (decimal?) and Status -> EvaluatedStatus to
//                  match the aggregate's renamed business fields (Status was freed for the new
//                  lifecycle enum). No external consumers existed for this event prior to this
//                  rename (verified by search). Additive-only evolution.
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when an EnvironmentalHealth aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.Events;

/// <summary>Raised when a new <see cref="EnvironmentalHealth"/> aggregate is created.</summary>
public sealed record EnvironmentalHealthCreatedEvent(
    Guid          EventId,
    Guid          EnvironmentalHealthId,
    Guid          TenantId,
    string        MonitoringType,
    decimal?      MeasuredValue,
    string        EvaluatedStatus,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IEnvironmentalHealthDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalHealthCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
