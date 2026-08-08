// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Shared
//  Layer:          Domain
//  File:           Result.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Minimal Domain-layer result wrapper shared by every consensus-standards
//                  (ANSI/ASME/ISO) compliance engine under Compliance.Standards (RiskAssessment,
//                  OperationalSafety, PressureVessel, and -- built by a parallel effort --
//                  CraneAndHoist, ControlSystemSafety, MachineSafety). Mirrors
//                  PlantSafety.Domain.Compliance.EPA.Shared.Result&lt;T&gt; exactly (same minimal
//                  shape) but is declared independently rather than reused, because Compliance.EPA
//                  and Compliance.Standards are sibling regulatory-domain areas and this codebase's
//                  "no bleed across module boundaries" convention treats each Compliance.* program
//                  family as its own self-contained area with its own Shared folder (see
//                  Compliance.EPA.Shared.Result for the precedent this mirrors). Intentionally NOT
//                  the richer Application-layer PlantSafety.Application.Common.Results.Result&lt;T&gt;/
//                  ApplicationError pattern -- importing that type into PlantSafety.Domain would be
//                  a layering violation. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.Standards.Shared;

/// <summary>Minimal Domain-layer result wrapper for the consensus-standards (ANSI/ASME/ISO) compliance engines.</summary>
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
