/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               BaseException.cs
    DESCRIPTION:        Canonical base exception for all platform exceptions. Wraps an Error instance, enforces SIEM-safe
                        messaging, and provides deterministic exception metadata. MUST NOT be thrown directly.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;
using PlantSafety.SharedKernel.Primitives.Errors;

namespace PlantSafety.SharedKernel.Primitives.Exceptions;

/// <summary>
/// Canonical base exception for all platform exceptions.
/// MUST NOT be thrown directly.
/// </summary>
public abstract class BaseException : Exception
{
    /// <summary>
    /// Canonical error instance associated with this exception.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Severity classification for observability and SIEM pipelines.
    /// </summary>
    public ExceptionSeverity Severity { get; }

    protected BaseException(Error error, ExceptionSeverity severity, Exception? inner = null)
        : base(error.Message, inner)
    {
        Error = error;
        Severity = severity;
    }

    public override string ToString()
        => $"{Error.Code.Value} ({Severity}): {Error.Message}";
}
