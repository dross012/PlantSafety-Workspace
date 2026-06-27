/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               ResultOfT.cs
    DESCRIPTION:        Canonical generic Result primitive. Represents success or failure with a value and strong-typed
                        Error. Enforces invariants: Success => Value set, Error null; Failure => Value default, Error set.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Primitives.Errors;

namespace PlantSafety.SharedKernel.Primitives.Results;

/// <summary>
/// Canonical generic Result primitive.
/// </summary>
public sealed class Result<T>
{
    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Value for successful results. Default when failed.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Strong-typed error for failed results. Null when successful.
    /// </summary>
    public Error? Error { get; }

    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result<T> Failure(Error error) => new(false, default, error);

    public override string ToString()
        => IsSuccess ? $"Success: {Value}" : $"Failure: {Error}";
}
