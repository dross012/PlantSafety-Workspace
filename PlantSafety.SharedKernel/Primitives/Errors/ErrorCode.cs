/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               ErrorCode.cs
    DESCRIPTION:        Strong-typed canonical error code. MUST be normalized, SIEM-safe, lowercase, dot-delimited, and
                        deterministic. Used for logs, telemetry, and API responses.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Primitives.Errors;

/// <summary>
/// Strong-typed canonical error code.
/// </summary>
public sealed record ErrorCode
{
    /// <summary>
    /// Normalized error code value (lowercase, dot-delimited).
    /// </summary>
    public string Value { get; }

    private ErrorCode(string value)
    {
        Value = value;
    }

    public static ErrorCode Create(string value)
    {
        value = value?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(value));

        if (value.Length == 0)
            throw new ArgumentException("ErrorCode cannot be empty.");

        if (!value.Contains('.'))
            throw new ArgumentException("ErrorCode must be dot-delimited (e.g., validation.required).");

        return new ErrorCode(value);
    }

    public override string ToString() => Value;
}
