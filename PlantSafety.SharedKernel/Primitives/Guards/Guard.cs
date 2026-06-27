/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               Guard.cs
    DESCRIPTION:        Canonical guard entry point. Provides strongly-typed, SIEM-safe invariant checks for null, empty,
                        and general boolean conditions. Delegates to specialized guard classes for strings, numbers, and
                        collections.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Exceptions;

namespace PlantSafety.SharedKernel.Primitives.Guards;

/// <summary>
/// Canonical guard entry point for invariant checking.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throws a <see cref="DomainException"/> if the value is null.
    /// </summary>
    public static void AgainstNull(object? value, string name)
    {
        if (value is null)
            throw new DomainException(CommonErrors.NullValue);
    }

    /// <summary>
    /// Throws a <see cref="DomainException"/> if the condition is true.
    /// </summary>
    public static void Against(bool condition, Error error)
    {
        if (condition)
            throw new DomainException(error);
    }
}
