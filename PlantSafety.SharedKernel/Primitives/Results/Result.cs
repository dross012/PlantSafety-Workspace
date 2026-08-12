/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               ResultOfT.cs
    DESCRIPTION:        Canonical generic Result primitive. Represents success or failure with a value and strong-typed
                        Error. Enforces invariants: Success => Value set, Error null; Failure => Value default, Error set.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System.Diagnostics.CodeAnalysis;

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
    /// <remarks>
    /// The MemberNotNullWhen attribute states, to the compiler, the invariant this file's header
    /// has always documented in prose: success implies Value is set. Without it every correct
    /// caller -- one that checks IsSuccess and only then reads Value -- still produced a
    /// nullable-dereference warning, and the usual way that gets silenced is the null-forgiving
    /// operator, which suppresses genuine bugs just as effectively as false ones.
    /// </remarks>
    /// <remarks>
    /// Both halves of the invariant are declared on BOTH properties. Annotating only
    /// IsSuccess=>Value and IsFailure=>Error left the common shapes unhelped: a caller writing
    /// `if (!IsSuccess) ... Error.Code` was not told Error is non-null, because that fact was
    /// attached to IsFailure, and the compiler does not connect the two.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    /// <remarks>Failure implies Error is set, the other half of the documented invariant.</remarks>
    [MemberNotNullWhen(true, nameof(Error))]
    [MemberNotNullWhen(false, nameof(Value))]
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

    /// <summary>
    /// Creates a failed result using the canonical "resource not found" error.
    /// </summary>
    public static Result<T> NotFound() => new(false, default, CommonErrors.NotFound);

    public override string ToString()
        => IsSuccess ? $"Success: {Value}" : $"Failure: {Error}";
}
