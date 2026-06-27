/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-25
    FILE:               NonEmptyString.cs
    DESCRIPTION:        Canonical immutable string value type representing a non-empty, trimmed string. No internal
                        validation exceptions; creation is explicit and caller-controlled.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

/// <summary>
/// Immutable value type representing a non-empty, trimmed string.
/// </summary>
public readonly record struct NonEmptyString
{
    /// <summary>
    /// Underlying string value (never null, may be empty only for the Empty instance).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Indicates whether this instance represents the empty sentinel.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public static readonly NonEmptyString Empty = new(string.Empty);

    public NonEmptyString(string value)
    {
        Value = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Attempts to create a NonEmptyString from the provided value.
    /// Returns false if the value is null, empty, or whitespace.
    /// </summary>
    public static bool TryFrom(string? value, out NonEmptyString result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = Empty;
            return false;
        }

        result = new NonEmptyString(value.Trim());
        return true;
    }

    public override string ToString() => Value;
}
