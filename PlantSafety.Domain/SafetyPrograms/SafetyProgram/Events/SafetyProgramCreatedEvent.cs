// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           SafetyProgramCreatedEvent.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Raised when a new SafetyProgram aggregate is created.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Events;

/// <summary>Raised when a new SafetyProgram aggregate is created.</summary>
public sealed record SafetyProgramCreatedEvent(
    Guid EventId,
    SafetyProgramId SafetyProgramId,
    UtcTimestamp CreatedAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : ISafetyProgramDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SafetyProgramCreated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
    /// <inheritdoc/>
    public UtcTimestamp OccurredAtUtc => CreatedAtUtc;
}
