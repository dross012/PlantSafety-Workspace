// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.PredictiveScheduling
//  Layer:          Domain
//  File:           OptimizationRunFactory.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for requesting OptimizationRun aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Aggregates;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Enums;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.Interfaces;
using PlantSafety.Domain.Scheduling.PredictiveScheduling.ValueObjects;

namespace PlantSafety.Domain.Scheduling.PredictiveScheduling.Factories;

/// <summary>Default factory implementation for requesting OptimizationRun aggregates.</summary>
public sealed class OptimizationRunFactory : IOptimizationRunFactory
{
    /// <inheritdoc/>
    public OptimizationRun Request(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<SchedulableTask> inputTasks,
        IReadOnlyList<ResourceAvailabilityWindow> inputAvailability,
        OptimizationObjective objective,
        UserId? requestedBy,
        UtcTimestamp nowUtc,
        CorrelationId correlationId)
        => OptimizationRun.Request(tenantId, siteId, inputTasks, inputAvailability, objective, requestedBy, nowUtc, correlationId);
}
