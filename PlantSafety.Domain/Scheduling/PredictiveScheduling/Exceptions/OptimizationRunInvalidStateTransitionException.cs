// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunInvalidStateTransitionException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a caller attempts an OptimizationRun lifecycle transition that is
//                  not permitted from the aggregate's current OptimizationRunStatus.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Exceptions;

/// <summary>Thrown when an OptimizationRun lifecycle transition is attempted from an incompatible status.</summary>
public sealed class OptimizationRunInvalidStateTransitionException : OptimizationRunDomainException
{
    /// <summary>Creates an invalid-state-transition exception.</summary>
    public OptimizationRunInvalidStateTransitionException(
        OptimizationRunId id, string currentStatus, string attemptedTransition, CorrelationId correlationId, CausationId causationId)
        : base(
            $"OptimizationRun {id} cannot perform transition '{attemptedTransition}' from status '{currentStatus}'.",
            "PredictiveScheduling.OptimizationRun.InvalidStateTransition",
            "Validation",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
