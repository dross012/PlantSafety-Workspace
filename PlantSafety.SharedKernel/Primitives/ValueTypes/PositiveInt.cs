/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-25
    FILE:               PositiveInt.cs
    DESCRIPTION:        Canonical immutable value type representing a positive integer (greater than zero). No internal
                        exceptions on creation via TryFrom; direct ctor is allowed for trusted paths.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Primitives.ValueTypes;

/// <summary>
/// Immutable value type representing a positive integer (greater than zero).
/// </summary>
public readonly record struct PositiveInt
{
    /// <summary>
    /// Underlying integer value.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Indicates whether this instance represents the empty sentinel.
    /// </summary>
    public bool IsEmpty => Value <= 0;

    public static readonly PositiveInt Empty = new(0);

    public PositiveInt(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Attempts to create a PositiveInt from the provided value.
    /// Returns false if the value is less than or equal to zero.
    /// </summary>
    public static bool TryFrom(int value, out PositiveInt result)
    {
        if (value <= 0)
        {
            result = Empty;
            return false;
        }

        result = new PositiveInt(value);
        return true;
    }

    public override string ToString() => Value.ToString();
}
