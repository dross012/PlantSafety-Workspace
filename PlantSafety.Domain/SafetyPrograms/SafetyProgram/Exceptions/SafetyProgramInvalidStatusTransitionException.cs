// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPrograms.SafetyProgram
//  Layer:          Domain
//  File:           SafetyProgramInvalidStatusTransitionException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when an illegal lifecycle transition is attempted on a SafetyProgram.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;

namespace PlantSafety.Domain.SafetyPrograms.SafetyProgram.Exceptions;

/// <summary>Thrown when an illegal lifecycle transition is attempted on a SafetyProgram.</summary>
public sealed class SafetyProgramInvalidStatusTransitionException : SafetyProgramDomainException
{
    /// <summary>Initialises the exception describing the rejected transition.</summary>
    public SafetyProgramInvalidStatusTransitionException(
        SafetyProgramId safetyProgramId,
        SafetyProgramStatus fromStatus,
        SafetyProgramStatus toStatus,
        CorrelationId correlationId,
        CausationId causationId)
        : base(
            $"SafetyProgram '{safetyProgramId}' cannot transition from '{fromStatus}' to '{toStatus}'.",
            "SafetyPrograms.SafetyProgram.InvalidStatusTransition",
            "Domain",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
