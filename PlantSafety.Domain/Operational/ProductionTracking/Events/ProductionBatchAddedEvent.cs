// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionBatchAddedEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raised when a Batch is added to a ProductionRun. Backs
//                  GetRunBatchesQuery/ListBatchesQuery. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.Enums;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.ProductionTracking.Events;

/// <summary>Raised when a Batch is added to a ProductionRun.</summary>
public sealed record ProductionBatchAddedEvent(
    Guid EventId,
    ProductionRunId ProductionRunId,
    BatchId BatchId,
    string BatchCode,
    BatchStatus Status,
    UtcTimestamp StartedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId) : IProductionTrackingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProductionBatchAdded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => StartedAtUtc;
}
