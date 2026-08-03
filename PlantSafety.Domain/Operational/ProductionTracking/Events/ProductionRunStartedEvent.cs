// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionRunStartedEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raised when a new ProductionRun is started. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.ProductionTracking.Events;

/// <summary>Raised when a new ProductionRun is started.</summary>
public sealed record ProductionRunStartedEvent(
    Guid EventId,
    ProductionRunId ProductionRunId,
    ProductionLineId LineId,
    Guid MachineId,
    string RunCode,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId) : IProductionTrackingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProductionRunStarted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}
