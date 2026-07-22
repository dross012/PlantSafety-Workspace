// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing Risk aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.RiskManagement.Aggregates;
using PlantSafety.Domain.RiskManagement.Interfaces;

namespace PlantSafety.Domain.RiskManagement.Factories;

/// <summary>Default factory implementation for constructing Risk aggregates.</summary>
public sealed class RiskFactory : IRiskFactory
{
    /// <inheritdoc/>
    public Risk Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => Risk.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}