/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               ErrorFactory.cs
    DESCRIPTION:        Factory for creating canonical Error instances. Ensures normalization, SIEM-safety, and
                        deterministic construction.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Primitives.Errors;

/// <summary>
/// Factory for constructing canonical <see cref="Error"/> instances.
/// </summary>
public static class ErrorFactory
{
    /// <summary>
    /// Creates a new canonical error.
    /// </summary>
    public static Error Create(string code, ErrorType type, string message)
        => new(ErrorCode.Create(code), type, message.Trim());
}
