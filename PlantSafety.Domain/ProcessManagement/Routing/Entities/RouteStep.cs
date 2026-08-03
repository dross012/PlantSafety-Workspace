// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Domain
//  File:           RouteStep.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Child entity of the RouteDefinition aggregate root representing a single
//                  ordered step in a manufacturing/operational route. Genuinely new -- Process-
//                  Definition has no notion of ordered structural content at all (its Commands
//                  only manage lifecycle status). ComputeStatus() is a real, deterministic domain
//                  rule (not a client-settable field): Blocked when the step carries no
//                  instructions, Warning when it has instructions but no assigned resources,
//                  otherwise Valid. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.ProcessManagement.Routing.Enums;

namespace PlantSafety.Domain.ProcessManagement.Routing.Entities;

/// <summary>A single ordered step within a <c>RouteDefinition</c>.</summary>
public sealed class RouteStep
{
    /// <summary>Stable identity, preserved across revisions when this step is edited in place.</summary>
    public Guid StepId { get; private set; }

    /// <summary>1-based sequential position within the route (no gaps).</summary>
    public int StepNumber { get; private set; }

    /// <summary>Step display name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Step execution instructions.</summary>
    public string Instructions { get; private set; } = string.Empty;

    /// <summary>Assigned resource names for this step.</summary>
    public IReadOnlyList<string> Resources { get; private set; } = [];

    /// <summary>Estimated execution duration, in minutes. Used by <c>RouteSimulationEngine</c>.</summary>
    public int EstimatedDurationMinutes { get; private set; }

    private RouteStep() { }

    /// <summary>Creates a brand-new step with a freshly generated <see cref="StepId"/>.</summary>
    public static RouteStep Create(int stepNumber, string name, string instructions, IReadOnlyList<string> resources, int estimatedDurationMinutes)
        => Rehydrate(Guid.NewGuid(), stepNumber, name, instructions, resources, estimatedDurationMinutes);

    /// <summary>Reconstructs a step with an explicit, already-known <see cref="StepId"/> (persistence rehydration or in-place revision).</summary>
    public static RouteStep Rehydrate(Guid stepId, int stepNumber, string name, string instructions, IReadOnlyList<string> resources, int estimatedDurationMinutes)
    {
        if (stepNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(stepNumber), stepNumber, "StepNumber must be 1-based (>= 1).");

        return new RouteStep
        {
            StepId = stepId == Guid.Empty ? Guid.NewGuid() : stepId,
            StepNumber = stepNumber,
            Name = (name ?? string.Empty).Trim(),
            Instructions = (instructions ?? string.Empty).Trim(),
            Resources = resources ?? [],
            EstimatedDurationMinutes = Math.Max(0, estimatedDurationMinutes),
        };
    }

    /// <summary>
    /// Deterministically classifies this step's readiness: <see cref="RouteStepStatus.Blocked"/>
    /// when no instructions are present, <see cref="RouteStepStatus.Warning"/> when instructions
    /// are present but no resources are assigned, otherwise <see cref="RouteStepStatus.Valid"/>.
    /// </summary>
    public RouteStepStatus ComputeStatus()
    {
        if (string.IsNullOrWhiteSpace(Instructions))
            return RouteStepStatus.Blocked;

        return Resources.Count == 0 ? RouteStepStatus.Warning : RouteStepStatus.Valid;
    }

    /// <summary>Projects this step into an immutable <see cref="ValueObjects.RouteStepSnapshot"/> for version capture.</summary>
    public ValueObjects.RouteStepSnapshot ToSnapshot() => new(StepId, StepNumber, Name, Instructions, Resources, EstimatedDurationMinutes);
}
