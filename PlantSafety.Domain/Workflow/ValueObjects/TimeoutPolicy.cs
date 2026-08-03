// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           TimeoutPolicy.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Value object describing the timeout window and resulting action for a workflow
//                  step (Phase 5.2 timeout rules: "a workflow instance can time out a step and
//                  escalate"). Deterministic: never reads the wall clock itself -- callers supply
//                  both the step's activation instant and "now".
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Enums;

namespace PlantSafety.Domain.Workflow.ValueObjects;

/// <summary>Timeout window and resulting action for a workflow step.</summary>
public readonly record struct TimeoutPolicy
{
    /// <summary>Duration after step activation at which the step is considered timed out.</summary>
    public TimeSpan Duration { get; }

    /// <summary>Action to take once the step has timed out.</summary>
    public TimeoutAction Action { get; }

    /// <summary>Creates a validated timeout policy.</summary>
    public TimeoutPolicy(TimeSpan duration, TimeoutAction action)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be positive.");

        Duration = duration;
        Action = action;
    }

    /// <summary>The no-timeout policy sentinel (steps with this policy never time out).</summary>
    public static TimeoutPolicy? None => null;

    /// <summary>Computes the UTC instant at which a step activated at <paramref name="activatedAtUtc"/> will time out.</summary>
    public UtcTimestamp ComputeTimeoutAtUtc(UtcTimestamp activatedAtUtc)
        => UtcTimestamp.From(activatedAtUtc.Value.Add(Duration));

    /// <summary>Returns <see langword="true"/> when a step activated at <paramref name="activatedAtUtc"/> has timed out as of <paramref name="nowUtc"/>.</summary>
    public bool HasTimedOut(UtcTimestamp activatedAtUtc, UtcTimestamp nowUtc)
        => nowUtc.Value >= ComputeTimeoutAtUtc(activatedAtUtc).Value;
}
