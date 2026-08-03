// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           IOptimizationRunFactory.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Factory abstraction for constructing OptimizationRun aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Aggregates;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Interfaces;

/// <summary>Factory abstraction for constructing <see cref="OptimizationRun"/> aggregates.</summary>
public interface IOptimizationRunFactory
{
    /// <summary>Creates a new OptimizationRun in the Pending status from the supplied input snapshot.</summary>
    OptimizationRun Request(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<SchedulableTask> inputTasks,
        IReadOnlyList<ResourceAvailabilityWindow> inputAvailability,
        OptimizationObjective objective,
        UserId? requestedBy,
        UtcTimestamp nowUtc,
        CorrelationId correlationId);
}
