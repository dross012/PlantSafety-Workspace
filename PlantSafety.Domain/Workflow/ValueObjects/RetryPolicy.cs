// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           RetryPolicy.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Value object describing bounded retry-with-backoff behavior for a workflow
//                  step (Phase 5.2 retry rules). Deterministic: never sleeps or reads the wall
//                  clock itself -- computes a next-attempt UtcTimestamp from a caller-supplied
//                  "now" instant.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Enums;

namespace PlantSafety.Domain.Workflow.ValueObjects;

/// <summary>Bounded retry-with-backoff configuration for a workflow step.</summary>
public readonly record struct RetryPolicy
{
    /// <summary>Maximum number of retry attempts permitted (0 disables retry entirely).</summary>
    public int MaxAttempts { get; }

    /// <summary>Deterministic backoff computation strategy.</summary>
    public RetryBackoffStrategy Strategy { get; }

    /// <summary>Base delay used as the seed for the backoff computation.</summary>
    public TimeSpan BaseDelay { get; }

    /// <summary>Creates a validated retry policy.</summary>
    public RetryPolicy(int maxAttempts, RetryBackoffStrategy strategy, TimeSpan baseDelay)
    {
        if (maxAttempts < 0)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts), maxAttempts, "MaxAttempts must be >= 0.");
        if (baseDelay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(baseDelay), baseDelay, "BaseDelay must be non-negative.");

        MaxAttempts = maxAttempts;
        Strategy = strategy ?? RetryBackoffStrategy.Default;
        BaseDelay = baseDelay;
    }

    /// <summary>The no-retry policy (MaxAttempts = 0).</summary>
    public static RetryPolicy None => new(0, RetryBackoffStrategy.None, TimeSpan.Zero);

    /// <summary>Returns <see langword="true"/> when another retry attempt is permitted given <paramref name="attemptsMade"/> so far.</summary>
    public bool CanRetry(int attemptsMade) => attemptsMade < MaxAttempts;

    /// <summary>
    /// Deterministically computes the UTC instant the next retry (1-based
    /// <paramref name="nextAttemptNumber"/>) should occur, relative to <paramref name="nowUtc"/>.
    /// </summary>
    public UtcTimestamp ComputeNextAttemptUtc(int nextAttemptNumber, UtcTimestamp nowUtc)
        => UtcTimestamp.From(nowUtc.Value.Add(Strategy.ComputeDelay(nextAttemptNumber, BaseDelay)));
}
