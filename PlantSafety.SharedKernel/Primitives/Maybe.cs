// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Primitives
//  Layer:          SharedKernel
//  File:           Maybe.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Maybe<T> option monad. Replaces nullable reference types
//                  for optional domain values where absence is semantically
//                  meaningful (not an error). Additive-only evolution.
// ============================================================================
namespace PlantSafety.SharedKernel.Primitives;

/// <summary>
/// Option monad wrapping an optional value. Use when absence is a valid,
/// non-error domain condition.
/// </summary>
/// <typeparam name="T">The wrapped type.</typeparam>
public sealed class Maybe<T>
{
    private readonly T? _value;

    public bool HasValue { get; }
    public bool HasNoValue => !HasValue;

    private Maybe() { HasValue = false; }
    private Maybe(T value) { _value = value; HasValue = true; }

    public static readonly Maybe<T> None = new();
    public static Maybe<T> From(T? value)
        => value is null ? None : new Maybe<T>(value);
    public static implicit operator Maybe<T>(T? value) => From(value);

    /// <summary>Returns the wrapped value or throws if empty.</summary>
    public T Value => HasValue ? _value! : throw new InvalidOperationException("Maybe<T> has no value.");

    /// <summary>Returns the wrapped value or <paramref name="defaultValue"/> if empty.</summary>
    public T GetValueOrDefault(T defaultValue) => HasValue ? _value! : defaultValue;

    /// <summary>Transforms the value if present.</summary>
    public Maybe<TResult> Map<TResult>(Func<T, TResult> selector)
        => HasValue ? Maybe<TResult>.From(selector(_value!)) : Maybe<TResult>.None;

    /// <summary>Executes <paramref name="action"/> if a value is present.</summary>
    public void IfPresent(Action<T> action) { if (HasValue) action(_value!); }

    public override string ToString() => HasValue ? _value!.ToString() ?? string.Empty : "None";
}
