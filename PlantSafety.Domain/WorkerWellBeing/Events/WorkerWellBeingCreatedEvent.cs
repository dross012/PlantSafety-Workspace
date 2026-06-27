// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           WorkerWellBeingCreatedEvent.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Domain event raised when a WorkerWellBeing aggregate is created.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.WorkerWellBeing.Events;

/// <summary>Raised when a new <see cref="WorkerWellBeing"/> aggregate is created.</summary>
public sealed record WorkerWellBeingCreatedEvent(
    Guid          EventId,
    Guid          WorkerWellBeingId,
    UtcTimestamp  CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId   CausationId
) : IWorkerWellBeingDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WorkerWellBeingCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
