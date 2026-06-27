// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Primitives
//  Layer:          SharedKernel
//  File:           Result.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Non-generic Result primitive (success or failure with no value).
//                  Complements Result<T> for operations that return void on success.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.Errors;

namespace PlantSafety.SharedKernel.Primitives;

/// <summary>
/// Non-generic Result for operations that succeed or fail without returning a value.
/// </summary>
public sealed class Result
{
    public bool IsSuccess  { get; }
    public bool IsFailure  => !IsSuccess;
    public Error? Error    { get; }

    private Result(bool isSuccess, Error? error) { IsSuccess = isSuccess; Error = error; }

    public static Result Success()          => new(true, null);
    public static Result Failure(Error err) => new(false, err);

    /// <summary>Converts to a <see cref="Result{T}"/> by projecting on success.</summary>
    public Results.Result<T> Map<T>(Func<T> valueFactory)
        => IsSuccess
            ? Results.Result<T>.Success(valueFactory())
            : Results.Result<T>.Failure(Error!);

    public override string ToString() => IsSuccess ? "Success" : $"Failure: {Error}";
}
