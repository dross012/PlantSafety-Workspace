// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleAsset
//  Layer:          Domain
//  File:           AutonomousVehicleAssetRegisteredEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the AutonomousVehicleAsset lifecycle: registration,
//                  autonomy-mode transitions, suspension, reactivation, and retirement.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleAsset.Events;

/// <summary>Raised when a new autonomous vehicle/mobile-robot asset is registered.</summary>
public sealed record AutonomousVehicleAssetRegisteredEvent(
    AutonomousVehicleAssetEventId EventId,
    AutonomousVehicleAssetId AutonomousVehicleAssetId,
    AutonomousVehicleType VehicleType,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAutonomousVehicleAssetDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutonomousVehicleAssetRegistered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an asset's autonomy mode transitions (e.g. Manual to Autonomous).</summary>
public sealed record AutonomyModeTransitionedEvent(
    AutonomousVehicleAssetEventId EventId,
    AutonomousVehicleAssetId AutonomousVehicleAssetId,
    AutonomyMode FromMode,
    AutonomyMode ToMode,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAutonomousVehicleAssetDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutonomyModeTransitioned";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when autonomous operation of an asset is suspended pending review.</summary>
public sealed record AutonomousOperationSuspendedEvent(
    AutonomousVehicleAssetEventId EventId,
    AutonomousVehicleAssetId AutonomousVehicleAssetId,
    string Reason,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAutonomousVehicleAssetDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutonomousOperationSuspended";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a previously suspended asset is reactivated.</summary>
public sealed record AutonomousVehicleAssetReactivatedEvent(
    AutonomousVehicleAssetEventId EventId,
    AutonomousVehicleAssetId AutonomousVehicleAssetId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAutonomousVehicleAssetDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutonomousVehicleAssetReactivated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an asset is permanently retired.</summary>
public sealed record AutonomousVehicleAssetRetiredEvent(
    AutonomousVehicleAssetEventId EventId,
    AutonomousVehicleAssetId AutonomousVehicleAssetId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAutonomousVehicleAssetDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutonomousVehicleAssetRetired";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
