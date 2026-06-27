/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               DomainException.cs
    DESCRIPTION:        Canonical domain exception. Represents violations of domain invariants, business rules, or
                        aggregate state transitions. MUST NOT be used for validation or infrastructure failures.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Primitives.Errors;

namespace PlantSafety.SharedKernel.Primitives.Exceptions;

/// <summary>
/// Represents a violation of a domain invariant or business rule.
/// </summary>
public sealed class DomainException : BaseException
{
    public DomainException(Error error, Exception? inner = null)
        : base(error, ExceptionSeverity.Major, inner)
    {
    }
}
