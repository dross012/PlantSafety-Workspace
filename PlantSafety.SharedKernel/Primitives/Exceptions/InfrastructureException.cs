/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               InfrastructureException.cs
    DESCRIPTION:        Canonical infrastructure exception. Represents failures in external systems, I/O, networking,
                        storage, messaging, or database operations. MUST NOT leak provider-specific details.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Primitives.Errors;

namespace PlantSafety.SharedKernel.Primitives.Exceptions;

/// <summary>
/// Represents a failure in an external system or infrastructure component.
/// </summary>
public sealed class InfrastructureException : BaseException
{
    public InfrastructureException(Error error, Exception? inner = null)
        : base(error, ExceptionSeverity.Critical, inner)
    {
    }
}
