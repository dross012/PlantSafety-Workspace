// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           AccidentFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Accident aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AccidentManagement.Aggregates;
using PlantSafety.Domain.AccidentManagement.Interfaces;

namespace PlantSafety.Domain.AccidentManagement.Factories;

/// <summary>Default factory implementation for constructing Accident aggregates.</summary>
public sealed class AccidentFactory : IAccidentFactory
{
    /// <inheritdoc/>
    public Accident Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Accident.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}