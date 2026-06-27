// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Canonical Result<T> type for all command handler return values.
//   Represents either a successful outcome with a value or a failure with an ApplicationError.
//   All command handlers MUST return Result<T>; exceptions are only for truly exceptional conditions.
//   Immutable, deterministic, SIEM-safe, additive-only evolution.
// SchemaVersion: 1
// Additive-only evolution: new factory methods may be added; no existing constructors or properties may change.

using PlantSafety.Application.Common.Errors;

namespace PlantSafety.Application.Common.Results;

/// <summary>
/// Canonical discriminated union representing the outcome of an Application layer operation.
/// On success: contains the value <typeparamref name="T"/>.
/// On failure: contains an <see cref="ApplicationError"/> with error code, message, and correlation context.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly ApplicationError? _error;

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = null;
    }

    private Result(ApplicationError error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the success value. Throws <see cref="InvalidOperationException"/> if the result is a failure.</summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a failure result.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access Value on a failure Result.");

    /// <summary>Gets the error. Throws <see cref="InvalidOperationException"/> if the result is a success.</summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a success result.</exception>
    public ApplicationError Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a success Result.");

    /// <summary>Creates a success result with the given value.</summary>
    /// <param name="value">The success value.</param>
    /// <returns>A <see cref="Result{T}"/> in the success state.</returns>
    public static Result<T> Success(T value) => new(value);

    /// <summary>Creates a failure result with the given error.</summary>
    /// <param name="error">The application error describing the failure.</param>
    /// <returns>A <see cref="Result{T}"/> in the failure state.</returns>
    public static Result<T> Failure(ApplicationError error) => new(error);

    /// <summary>Implicit conversion from a value to a success result.</summary>
    /// <param name="value">The value to wrap.</param>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>Implicit conversion from an error to a failure result.</summary>
    /// <param name="error">The error to wrap.</param>
    public static implicit operator Result<T>(ApplicationError error) => Failure(error);

    /// <inheritdoc />
    public override string ToString() =>
        IsSuccess ? $"Success({_value})" : $"Failure({_error?.ErrorCode}: {_error?.Message})";
}

/// <summary>
/// Non-generic <see cref="Result"/> for operations that produce no value (void commands).
/// </summary>
public sealed class Result
{
    private readonly ApplicationError? _error;

    private Result() => IsSuccess = true;

    private Result(ApplicationError error)
    {
        IsSuccess = false;
        _error = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error. Throws <see cref="InvalidOperationException"/> if the result is a success.</summary>
    public ApplicationError Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a success Result.");

    /// <summary>The singleton success result.</summary>
    public static readonly Result Ok = new();

    /// <summary>Creates a failure result with the given error.</summary>
    /// <param name="error">The application error describing the failure.</param>
    /// <returns>A <see cref="Result"/> in the failure state.</returns>
    public static Result Failure(ApplicationError error) => new(error);

    /// <summary>Implicit conversion from an error to a failure result.</summary>
    /// <param name="error">The error to wrap.</param>
    public static implicit operator Result(ApplicationError error) => Failure(error);
}
