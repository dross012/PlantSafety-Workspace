// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           IAccidentFactory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing Accident aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.AccidentManagement.Aggregates;

namespace PlantSafety.Domain.AccidentManagement.Interfaces;

/// <summary>Factory interface for constructing Accident aggregates.</summary>
public interface IAccidentFactory
{
    /// <summary>Creates a new Accident aggregate with the supplied parameters.</summary>
    Accident Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}