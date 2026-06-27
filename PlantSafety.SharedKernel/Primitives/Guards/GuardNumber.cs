/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               GuardNumber.cs
    DESCRIPTION:        Canonical guard helpers for numeric invariants. Enforces positive, non-negative, range, and
                        comparison constraints. MUST remain SIEM-safe and deterministic.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Exceptions;

namespace PlantSafety.SharedKernel.Primitives.Guards;

/// <summary>
/// Guard helpers for numeric invariants.
/// </summary>
public static class GuardNumber
{
    public static void Positive(int value, string name)
    {
        if (value <= 0)
            throw new DomainException(
                ErrorFactory.Create("validation.positive", ErrorType.Validation,
                    $"{name} must be greater than zero."));
    }

    public static void NonNegative(int value, string name)
    {
        if (value < 0)
            throw new DomainException(
                ErrorFactory.Create("validation.nonnegative", ErrorType.Validation,
                    $"{name} must be zero or greater."));
    }

    public static void InRange(int value, int min, int max, string name)
    {
        if (value < min || value > max)
            throw new DomainException(
                ErrorFactory.Create("validation.range", ErrorType.Validation,
                    $"{name} must be between {min} and {max}."));
    }
}
