/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-28
    FILE:               GuardException.cs
    DESCRIPTION:        Guard helpers for exception invariants. Ensures exceptions are not null and provides canonical
                        wrapping behavior for domain-safe rethrows.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.SharedKernel.Primitives.Exceptions;

namespace PlantSafety.SharedKernel.Primitives.Guards;

/// <summary>
/// Guard helpers for exception invariants.
/// </summary>
public static class GuardException
{
    /// <summary>
    /// Ensures an exception instance is not null.
    /// </summary>
    public static void NotNull(Exception? ex)
    {
        if (ex is null)
            throw new DomainException(CommonErrors.NullValue);
    }

    /// <summary>
    /// Wraps an exception in a canonical <see cref="InfrastructureException"/> if needed.
    /// </summary>
    public static void WrapInfrastructure(Exception ex)
    {
        if (ex is InfrastructureException)
            throw ex;

        throw new InfrastructureException(
            ErrorFactory.Create("infrastructure.failure", ErrorType.DependencyFailure,
                "An infrastructure failure occurred."),
            ex);
    }
}
