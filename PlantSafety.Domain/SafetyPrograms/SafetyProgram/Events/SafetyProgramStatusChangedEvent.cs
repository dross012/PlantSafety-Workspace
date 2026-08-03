// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           SafetyProgramStatusChangedEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised whenever a SafetyProgram transitions to a new lifecycle status.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Events;

/// <summary>Raised whenever a SafetyProgram transitions to a new lifecycle status.</summary>
public sealed record SafetyProgramStatusChangedEvent(
    Guid EventId,
    SafetyProgramId SafetyProgramId,
    SafetyProgramStatus PreviousStatus,
    SafetyProgramStatus NewStatus,
    UtcTimestamp ChangedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyProgramDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyProgramStatusChanged";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => ChangedAtUtc;
}
