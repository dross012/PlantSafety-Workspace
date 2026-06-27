/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               ResultFactory.cs
    DESCRIPTION:        Factory helpers for constructing canonical Result instances from Error primitives and conditions.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using PlantSafety.SharedKernel.Primitives.Errors;

namespace PlantSafety.SharedKernel.Primitives.Results;

/// <summary>
/// Factory helpers for Result primitives.
/// </summary>
public static class ResultFactory
{
    /// <summary>
    /// Creates a Result based on a boolean condition.
    /// </summary>
    public static Result FromCondition(bool condition, Error error)
        => condition ? Result.Success() : Result.Failure(error);

    /// <summary>
    /// Creates a Result&lt;T&gt; based on a nullable value.
    /// </summary>
    public static Result<T> FromNullable<T>(T? value, Error error)
        => value is null ? Result<T>.Failure(error) : Result<T>.Success(value);

    /// <summary>
    /// Wraps a function in a Result&lt;T&gt;, converting thrown exceptions into a failure using the provided error.
    /// </summary>
    public static Result<T> FromFunc<T>(Func<T> func, Error errorOnException)
    {
        if (func is null) throw new ArgumentNullException(nameof(func));

        try
        {
            return Result<T>.Success(func());
        }
        catch
        {
            return Result<T>.Failure(errorOnException);
        }
    }
}
