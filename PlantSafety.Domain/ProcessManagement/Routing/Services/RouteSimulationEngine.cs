// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteSimulationEngine.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Pure, deterministic domain service that walks a RouteDefinition's ordered step
//                  sequence and produces a simulated execution outcome (total duration, critical
//                  path, resource-conflict count, material-shortage count, bottleneck steps).
//                  Genuinely new algorithmic work -- no critical-path/bottleneck-detection concept
//                  exists anywhere else in the platform (the UI's own ApiClient header explicitly
//                  requires this logic to live server-side). Deterministic: the same ordered step
//                  sequence always produces the same outcome. Additive-only evolution.
//
//  Simulation model (routes are strictly sequential -- no branching):
//   - TotalDurationMinutes: sum of every step's EstimatedDurationMinutes.
//   - CriticalPathStepNumbers: the full ordered step sequence (sequential routes have exactly one
//     path, so every step is on the critical path by definition).
//   - ResourceConflictCount: count of distinct resource names required by more than one step --
//     a deterministic proxy for cross-step scheduling contention on a shared resource.
//   - MaterialShortageCount: count of steps classified Blocked by RouteStep.ComputeStatus()
//     (missing instructions/readiness).
//   - BottleneckStepNumbers: steps whose EstimatedDurationMinutes exceeds 1.5x the route's mean
//     step duration.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.Entities;
using PlantSafety.Domain.ProcessManagement.Routing.Enums;
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;

namespace PlantSafety.Domain.ProcessManagement.Routing.Services;

/// <summary>Simulates execution of a <see cref="RouteDefinition"/>'s ordered step sequence.</summary>
public static class RouteSimulationEngine
{
    private const double BottleneckThresholdMultiplier = 1.5;

    /// <summary>Runs a deterministic what-if simulation over <paramref name="steps"/>, in step-number order.</summary>
    public static RouteSimulationOutcome Simulate(IReadOnlyList<RouteStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);

        if (steps.Count == 0)
            return new RouteSimulationOutcome(0, [], 0, 0, []);

        var ordered = steps.OrderBy(s => s.StepNumber).ToList();

        var totalDuration = ordered.Sum(s => s.EstimatedDurationMinutes);
        var criticalPath = ordered.Select(s => s.StepNumber).ToList();

        var resourceConflictCount = ordered
            .SelectMany(s => s.Resources.Distinct(StringComparer.OrdinalIgnoreCase).Select(r => (Resource: r, s.StepNumber)))
            .GroupBy(x => x.Resource, StringComparer.OrdinalIgnoreCase)
            .Count(g => g.Select(x => x.StepNumber).Distinct().Count() > 1);

        var materialShortageCount = ordered.Count(s => s.ComputeStatus() == RouteStepStatus.Blocked);

        var averageDuration = ordered.Count == 0 ? 0d : ordered.Average(s => s.EstimatedDurationMinutes);
        var bottleneckThreshold = averageDuration * BottleneckThresholdMultiplier;
        var bottleneckSteps = ordered
            .Where(s => s.EstimatedDurationMinutes > 0 && s.EstimatedDurationMinutes > bottleneckThreshold)
            .Select(s => s.StepNumber)
            .ToList();

        return new RouteSimulationOutcome(totalDuration, criticalPath, resourceConflictCount, materialShortageCount, bottleneckSteps);
    }
}
