// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionRunStatusChangedEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raised for every ProductionRun lifecycle transition other than start/completion
//                  (Pause/Resume). A single generic event keeps the event surface proportionate to
//                  this module's scope, mirroring WorkOrderStatusChangedEvent's precedent.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.Enums;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.ProductionTracking.Events;

/// <summary>Raised for a ProductionRun lifecycle transition (Pause/Resume).</summary>
public sealed record ProductionRunStatusChangedEvent(
    Guid EventId,
    ProductionRunId ProductionRunId,
    ProductionRunStatus PreviousStatus,
    ProductionRunStatus NewStatus,
    string? Reason,
    UtcTimestamp ChangedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId) : IProductionTrackingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProductionRunStatusChanged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ChangedAtUtc;
}
