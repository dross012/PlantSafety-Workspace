// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Shared
//  Layer:          Domain
//  File:           Result.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Minimal Domain-layer result wrapper used by the EPA compliance engines
//                  (RCRA, CERCLA/SARA, and -- built by a parallel effort -- TSCA/CAA/CWA).
//                  Intentionally NOT the richer Application-layer
//                  PlantSafety.Application.Common.Results.Result<T>/ApplicationError pattern --
//                  importing that type into PlantSafety.Domain would be a layering violation.
//                  This type exists only because no Domain-layer Result<T> currently exists in
//                  this codebase; if one is introduced later, these engines should be migrated to
//                  it. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.EPA.Shared;

/// <summary>Minimal Domain-layer result wrapper for the EPA compliance engines.</summary>
public sealed class Result<T>
{
    /// <summary><see langword="true"/> when the operation succeeded and <see cref="Value"/> is populated.</summary>
    public bool IsSuccess { get; }

    /// <summary>The successful result value, if <see cref="IsSuccess"/> is <see langword="true"/>.</summary>
    public T? Value { get; }

    /// <summary>The failure reason, if <see cref="IsSuccess"/> is <see langword="false"/>.</summary>
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Creates a successful result wrapping <paramref name="value"/>.</summary>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>Creates a failed result carrying <paramref name="error"/>.</summary>
    public static Result<T> Failure(string error) => new(false, default, error);
}
