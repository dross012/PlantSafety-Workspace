/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventCorrelation.cs
    DESCRIPTION:        Strong-typed correlation identifier ValueObject. Enforces deterministic lineage propagation and
                        SIEM-safe normalization.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Events.Base;

/// <summary>
/// Strong-typed correlation identifier for domain events.
/// <para>
/// Invariants:
/// - MUST NOT be null or empty.
/// - MUST be trimmed and normalized.
/// - MUST be stable across the entire request/operation.
/// </para>
/// </summary>
public sealed record EventCorrelation
{
    /// <summary>
    /// The normalized correlation identifier value.
    /// </summary>
    public string Value { get; }

    private EventCorrelation(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="EventCorrelation"/> after applying normalization and validation.
    /// </summary>
    /// <param name="value">Raw correlation identifier.</param>
    /// <returns>A valid <see cref="EventCorrelation"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
    public static EventCorrelation Create(string value)
    {
        value = value?.Trim() ?? throw new ArgumentNullException(nameof(value));

        if (value.Length == 0)
            throw new ArgumentException("CorrelationId cannot be empty.", nameof(value));

        return new EventCorrelation(value);
    }

    /// <summary>
    /// Attempts to parse a raw string into a valid <see cref="EventCorrelation"/> without throwing.
    /// </summary>
    /// <param name="value">Raw correlation identifier.</param>
    /// <param name="result">Parsed <see cref="EventCorrelation"/> if successful; otherwise null.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    public static bool TryParse(string? value, out EventCorrelation? result)
    {
        try
        {
            result = Create(value!);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}
