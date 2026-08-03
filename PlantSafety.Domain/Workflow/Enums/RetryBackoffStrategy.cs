// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           RetryBackoffStrategy.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the deterministic backoff computation used by a step's
//                  RetryPolicy (Phase 5.2 retry rules). Never uses wall-clock sleep; only computes
//                  a delay duration to be added to a supplied UtcTimestamp by the caller.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Workflow.Enums;

/// <summary>Smart Enum representing a deterministic retry backoff computation strategy.</summary>
public sealed class RetryBackoffStrategy : IEquatable<RetryBackoffStrategy>
{
    /// <summary>No retry is permitted.</summary>
    public static readonly RetryBackoffStrategy None = new("None");

    /// <summary>Every retry waits the same fixed base delay.</summary>
    public static readonly RetryBackoffStrategy Fixed = new("Fixed");

    /// <summary>Delay grows linearly with attempt number: baseDelay * attemptNumber.</summary>
    public static readonly RetryBackoffStrategy Linear = new("Linear");

    /// <summary>Delay grows exponentially with attempt number: baseDelay * 2^(attemptNumber-1).</summary>
    public static readonly RetryBackoffStrategy Exponential = new("Exponential");

    /// <summary>All defined RetryBackoffStrategy instances.</summary>
    public static readonly IReadOnlyList<RetryBackoffStrategy> All = [None, Fixed, Linear, Exponential];

    /// <summary>The default RetryBackoffStrategy instance, used when no explicit strategy is supplied.</summary>
    public static RetryBackoffStrategy Default => Fixed;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    private RetryBackoffStrategy(string code) => Code = code;

    /// <summary>
    /// Deterministically computes the delay to apply before the given 1-based
    /// <paramref name="attemptNumber"/>, given a configured <paramref name="baseDelay"/>. Pure
    /// function of its inputs -- never reads the wall clock.
    /// </summary>
    public TimeSpan ComputeDelay(int attemptNumber, TimeSpan baseDelay)
    {
        if (attemptNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(attemptNumber), attemptNumber, "Attempt number must be >= 1.");

        return Code switch
        {
            "None" => TimeSpan.Zero,
            "Fixed" => baseDelay,
            "Linear" => baseDelay * attemptNumber,
            "Exponential" => baseDelay * Math.Pow(2, attemptNumber - 1),
            _ => baseDelay,
        };
    }

    /// <summary>Resolves a RetryBackoffStrategy instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static RetryBackoffStrategy From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown RetryBackoffStrategy code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a RetryBackoffStrategy instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out RetryBackoffStrategy value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized RetryBackoffStrategy code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(RetryBackoffStrategy? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as RetryBackoffStrategy);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
