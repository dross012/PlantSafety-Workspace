// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunNotFoundException.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a referenced OptimizationRunId cannot be resolved.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects.Ids;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Exceptions;

/// <summary>Thrown when a referenced OptimizationRunId cannot be resolved.</summary>
public sealed class OptimizationRunNotFoundException : OptimizationRunDomainException
{
    /// <summary>Creates a run-not-found exception.</summary>
    public OptimizationRunNotFoundException(OptimizationRunId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"OptimizationRun {id} was not found.",
            "PredictiveScheduling.OptimizationRun.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
