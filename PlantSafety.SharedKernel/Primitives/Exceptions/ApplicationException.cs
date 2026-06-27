/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               ApplicationException.cs
    DESCRIPTION:        Canonical application exception. Represents expected, meaningful failures during application
                        workflows such as commands, queries, projections, routing, and notifications.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using PlantSafety.SharedKernel.Primitives.Errors;

namespace PlantSafety.SharedKernel.Primitives.Exceptions;

/// <summary>
/// Represents an expected failure in the application layer.
/// </summary>
public sealed class ApplicationException : BaseException
{
    public ApplicationException(Error error, Exception? inner = null)
        : base(error, ExceptionSeverity.Minor, inner)
    {
    }
}
