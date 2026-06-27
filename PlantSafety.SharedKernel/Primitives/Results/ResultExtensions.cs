/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               ResultExtensions.cs
    DESCRIPTION:        Canonical extension helpers for Result and Result<T>. Provides mapping, binding, and conversion
                        helpers while preserving strong-typed Error semantics.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using PlantSafety.SharedKernel.Primitives.Errors;

namespace PlantSafety.SharedKernel.Primitives.Results;

/// <summary>
/// Extension helpers for Result primitives.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a successful Result&lt;T&gt; to Result&lt;TOut&gt;; propagates failure unchanged.
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));
        if (mapper is null) throw new ArgumentNullException(nameof(mapper));

        return result.IsSuccess
            ? Result<TOut>.Success(mapper(result.Value!))
            : Result<TOut>.Failure(result.Error!);
    }

    /// <summary>
    /// Binds a successful Result&lt;T&gt; to another Result; propagates failure unchanged.
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> binder)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));
        if (binder is null) throw new ArgumentNullException(nameof(binder));

        return result.IsSuccess
            ? binder(result.Value!)
            : Result<TOut>.Failure(result.Error!);
    }

    /// <summary>
    /// Converts a non-generic Result to Result&lt;T&gt; with a provided value on success.
    /// </summary>
    public static Result<T> WithValue<T>(this Result result, T value)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));

        return result.IsSuccess
            ? Result<T>.Success(value)
            : Result<T>.Failure(result.Error!);
    }

    /// <summary>
    /// Converts Result&lt;T&gt; to a non-generic Result, discarding the value.
    /// </summary>
    public static Result ToNonGeneric<T>(this Result<T> result)
    {
        if (result is null) throw new ArgumentNullException(nameof(result));

        return result.IsSuccess
            ? Result.Success()
            : Result.Failure(result.Error!);
    }
}
