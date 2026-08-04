// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AutonomousVehicleSafety.AutonomousVehicleIncident
//  Layer:          Domain
//  File:           AutonomousVehicleIncidentRecordedEvent.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the AutonomousVehicleIncident lifecycle: recording,
//                  investigation start, suspension-review trigger, and closure.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AutonomousVehicleSafety.AutonomousVehicleIncident.Events;

/// <summary>Raised when a new autonomous vehicle incident is recorded.</summary>
public sealed record AutonomousVehicleIncidentRecordedEvent(
    AutonomousVehicleIncidentEventId EventId,
    AutonomousVehicleIncidentId AutonomousVehicleIncidentId,
    Guid AutonomousVehicleAssetId,
    AutonomousVehicleIncidentType IncidentType,
    IncidentSeverityLevel SeverityLevel,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAutonomousVehicleIncidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutonomousVehicleIncidentRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an incident investigation begins.</summary>
public sealed record AutonomousVehicleIncidentInvestigationStartedEvent(
    AutonomousVehicleIncidentEventId EventId,
    AutonomousVehicleIncidentId AutonomousVehicleIncidentId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAutonomousVehicleIncidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutonomousVehicleIncidentInvestigationStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when repeated incidents trigger an autonomous-operation suspension review.</summary>
public sealed record AutonomousVehicleIncidentSuspensionTriggeredEvent(
    AutonomousVehicleIncidentEventId EventId,
    AutonomousVehicleIncidentId AutonomousVehicleIncidentId,
    Guid AutonomousVehicleAssetId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAutonomousVehicleIncidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutonomousVehicleIncidentSuspensionTriggered";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an incident is closed.</summary>
public sealed record AutonomousVehicleIncidentClosedEvent(
    AutonomousVehicleIncidentEventId EventId,
    AutonomousVehicleIncidentId AutonomousVehicleIncidentId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAutonomousVehicleIncidentDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "AutonomousVehicleIncidentClosed";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}
