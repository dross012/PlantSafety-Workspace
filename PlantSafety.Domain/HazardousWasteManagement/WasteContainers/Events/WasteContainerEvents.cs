// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteContainers
//  Layer:          Domain
//  File:           WasteContainerEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Domain events raised across the WasteContainer lifecycle: open, close, shipped.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Events;

/// <summary>Raised when an accumulation container is opened.</summary>
public sealed record AccumulationContainerOpenedEvent(
    WasteContainerEventId EventId,
    WasteContainerId WasteContainerId,
    Guid AccumulationAreaId,
    Guid WasteStreamId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWasteContainerDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccumulationContainerOpened";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when an accumulation container is closed.</summary>
public sealed record AccumulationContainerClosedEvent(
    WasteContainerEventId EventId,
    WasteContainerId WasteContainerId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWasteContainerDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AccumulationContainerClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}

/// <summary>Raised when a container is marked shipped off-site under a manifest.</summary>
public sealed record WasteContainerShippedEvent(
    WasteContainerEventId EventId,
    WasteContainerId WasteContainerId,
    Guid ManifestId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IWasteContainerDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WasteContainerShipped";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.2";
}
