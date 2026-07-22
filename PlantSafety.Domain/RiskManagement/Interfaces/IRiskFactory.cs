// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           IRiskFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Risk aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.RiskManagement.Aggregates;

namespace PlantSafety.Domain.RiskManagement.Interfaces;

/// <summary>Factory interface for constructing Risk aggregates.</summary>
public interface IRiskFactory
{
    /// <summary>Creates a new Risk aggregate with the supplied parameters.</summary>
    Risk Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}