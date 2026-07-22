// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoringCreatedEvent.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new EnvironmentalAssetMonitoring aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Events;

/// <summary>Raised when a new <see cref="EnvironmentalAssetMonitoring"/> aggregate is created.</summary>
public sealed record EnvironmentalAssetMonitoringCreatedEvent(
    EnvironmentalAssetMonitoringEventId EventId,
    EnvironmentalAssetMonitoringId      EnvironmentalAssetMonitoringId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IEnvironmentalAssetMonitoringDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnvironmentalAssetMonitoringCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}