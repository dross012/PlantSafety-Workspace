// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Domain
//  File:           ProductionRunCompletedEvent.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Raised when a ProductionRun ends, either successfully (Completed) or abnormally
//                  (Failed). Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Operational.ProductionTracking.Enums;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Domain.Operational.ProductionTracking.Events;

/// <summary>Raised when a ProductionRun ends (Completed or Failed).</summary>
public sealed record ProductionRunCompletedEvent(
    Guid EventId,
    ProductionRunId ProductionRunId,
    ProductionRunStatus FinalStatus,
    int GoodUnitCount,
    int ScrapUnitCount,
    UtcTimestamp EndedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId) : IProductionTrackingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "ProductionRunCompleted";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => EndedAtUtc;
}
