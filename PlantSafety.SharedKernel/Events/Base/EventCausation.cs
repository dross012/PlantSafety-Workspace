/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               EventCausation.cs
    DESCRIPTION:        Strong-typed causation identifier ValueObject. Enforces deterministic lineage propagation and
                        SIEM-safe normalization. MUST follow Canonical Causation Rule.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Events.Base;

/// <summary>
/// Strong-typed causation identifier for domain events.
/// <para>
/// Invariants:
/// - MUST NOT be null or empty.
/// - MUST be trimmed and normalized.
/// - MUST represent the immediate parent operation or event.
/// </para>
/// </summary>
public sealed record EventCausation
{
    /// <summary>
    /// The normalized causation identifier value.
    /// </summary>
    public string Value { get; }

    private EventCausation(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="EventCausation"/> after applying normalization and validation.
    /// </summary>
    /// <param name="value">Raw causation identifier.</param>
    /// <returns>A valid <see cref="EventCausation"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is empty.</exception>
    public static EventCausation Create(string value)
    {
        value = value?.Trim() ?? throw new ArgumentNullException(nameof(value));

        if (value.Length == 0)
            throw new ArgumentException("CausationId cannot be empty.", nameof(value));

        return new EventCausation(value);
    }

    /// <summary>
    /// Attempts to parse a raw string into a valid <see cref="EventCausation"/> without throwing.
    /// </summary>
    /// <param name="value">Raw causation identifier.</param>
    /// <param name="result">Parsed <see cref="EventCausation"/> if successful; otherwise null.</param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    public static bool TryParse(string? value, out EventCausation? result)
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
