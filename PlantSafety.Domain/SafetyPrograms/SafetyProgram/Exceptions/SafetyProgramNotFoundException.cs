// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           SafetyProgramNotFoundException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a requested SafetyProgram aggregate cannot be located.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Exceptions;

/// <summary>Thrown when a requested SafetyProgram aggregate cannot be located.</summary>
public sealed class SafetyProgramNotFoundException : SafetyProgramDomainException
{
    /// <summary>Initialises the exception for the given missing identity.</summary>
    public SafetyProgramNotFoundException(SafetyProgramId safetyProgramId, CorrelationId correlationId, CausationId causationId)
        : base(
            $"SafetyProgram '{safetyProgramId}' was not found.",
            "SafetyPrograms.SafetyProgram.NotFound",
            "NotFound",
            "Low",
            correlationId,
            causationId)
    {
    }
}
