/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-25
    FILE:               UtcTimestamp.cs
    DESCRIPTION:        Canonical immutable value type representing a UTC DateTime. Ensures Kind == Utc and provides
                        a clear Empty sentinel.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

/// <summary>
/// Immutable value type representing a UTC timestamp.
/// </summary>
public readonly record struct UtcTimestamp
{
    /// <summary>
    /// Underlying DateTime value (Kind == Utc, unless Empty).
    /// </summary>
    public DateTime Value { get; }

    /// <summary>
    /// Indicates whether this instance represents the empty sentinel.
    /// </summary>
    public bool IsEmpty => Value == DateTime.MinValue;

    public static readonly UtcTimestamp Empty = new(DateTime.MinValue);

    public UtcTimestamp(DateTime value)
    {
        Value = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    /// <summary>
    /// Creates a UtcTimestamp from a DateTime, normalizing to UTC.
    /// </summary>
    public static UtcTimestamp From(DateTime value) => new(value);

    /// <summary>
    /// Returns the current UTC time as a UtcTimestamp.
    /// </summary>
    public static UtcTimestamp Now() => new(DateTime.UtcNow);

    public override string ToString() => IsEmpty ? string.Empty : Value.ToString("O");
}
