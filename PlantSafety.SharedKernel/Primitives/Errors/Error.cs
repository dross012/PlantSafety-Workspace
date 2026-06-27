/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               Error.cs
    DESCRIPTION:        Canonical error primitive. Represents a strong-typed, immutable, SIEM-safe error instance containing
                        an ErrorCode, ErrorType, and human-readable message. MUST follow the Canonical Error Envelope Rule.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. Existing members MUST NOT change semantics.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Primitives.Errors;

/// <summary>
/// Canonical error primitive used across all modules.
/// Immutable, strong-typed, and SIEM-safe.
/// </summary>
public sealed record Error
{
    /// <summary>
    /// Canonical error code (machine-readable).
    /// </summary>
    public ErrorCode Code { get; }

    /// <summary>
    /// Canonical error type (category).
    /// </summary>
    public ErrorType Type { get; }

    /// <summary>
    /// Human-readable, SIEM-safe message.
    /// </summary>
    public string Message { get; }

    public Error(ErrorCode code, ErrorType type, string message)
    {
        Code = code;
        Type = type;
        Message = message?.Trim() ?? string.Empty;
    }

    public override string ToString() => $"{Code.Value}: {Message}";
}

