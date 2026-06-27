/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               GuardString.cs
    DESCRIPTION:        Canonical guard helpers for string invariants. Enforces non-null, non-empty, non-whitespace, and
                        length constraints. MUST remain SIEM-safe and deterministic.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Exceptions;

namespace PlantSafety.SharedKernel.Primitives.Guards;

/// <summary>
/// Guard helpers for string invariants.
/// </summary>
public static class GuardString
{
    /// <summary>
    /// Ensures a string is not null, empty, or whitespace.
    /// </summary>
    public static void NotNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(CommonErrors.MissingRequiredField);
    }

    /// <summary>
    /// Ensures a string does not exceed a maximum length.
    /// </summary>
    public static void MaxLength(string value, int maxLength, string name)
    {
        if (value.Length > maxLength)
            throw new DomainException(
                ErrorFactory.Create("validation.maxlength", ErrorType.Validation,
                    $"{name} exceeds maximum length of {maxLength}."));
    }

    /// <summary>
    /// Ensures a string meets a minimum length.
    /// </summary>
    public static void MinLength(string value, int minLength, string name)
    {
        if (value.Length < minLength)
            throw new DomainException(
                ErrorFactory.Create("validation.minlength", ErrorType.Validation,
                    $"{name} must be at least {minLength} characters."));
    }
}
