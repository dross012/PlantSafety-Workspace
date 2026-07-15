// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Domain
//  File:           EnvironmentalHealthCreatedEvent.cs
//  Created:        2026-06-17
//  Updated:        2026-07-15 — added TenantId/MetricType/MeasurementValue/Status payload
//                  fields (additive) to match the extended EnvironmentalHealth aggregate shape.
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
    string        MetricType,
    decimal       MeasurementValue,
    string        Status,
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
