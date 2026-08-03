// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           SafetyProgramFactory.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing SafetyProgram aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Interfaces;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Factories;

/// <summary>Default factory implementation for constructing <see cref="Aggregates.SafetyProgram"/> aggregates.</summary>
public sealed class SafetyProgramFactory : ISafetyProgramFactory
{
    /// <inheritdoc/>
    public Aggregates.SafetyProgram Create(
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
        IReadOnlyList<string>? goals = null)
        => Aggregates.SafetyProgram.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor,
            tenantId, siteId, name, description, category, startDate, endDate, ownerId, goals);
}
