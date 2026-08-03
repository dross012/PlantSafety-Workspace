// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           ISafetyProgramFactory.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing SafetyProgram aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Interfaces;

/// <summary>Factory interface for constructing <see cref="Aggregates.SafetyProgram"/> aggregates.</summary>
public interface ISafetyProgramFactory
{
    /// <summary>Creates a new SafetyProgram aggregate with the supplied parameters.</summary>
    Aggregates.SafetyProgram Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId,
        string name,
        string description,
        SafetyProgramCategory category,
        DateOnly startDate,
        DateOnly? endDate,
        UserId ownerId,
        IReadOnlyList<string>? goals = null);
}
