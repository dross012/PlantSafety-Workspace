/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               ExceptionSeverity.cs
    DESCRIPTION:        Canonical severity classification for exceptions. Used by logging, telemetry, retry policies, and
                        SIEM pipelines. MUST remain additive-only.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

namespace PlantSafety.SharedKernel.Primitives.Exceptions;

/// <summary>
/// Canonical severity classification for exceptions.
/// </summary>
public enum ExceptionSeverity
{
    /// <summary>
    /// Informational exception. Represents expected, non-failure conditions such as cancellations,
    /// soft-fail workflows, or telemetry-only exceptions.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Minor exception. Represents expected application-level failures.
    /// </summary>
    Minor = 1,

    /// <summary>
    /// Major exception. Represents domain or business rule violations.
    /// </summary>
    Major = 2,

    /// <summary>
    /// Critical exception. Represents infrastructure failures, dependency failures,
    /// or unhandled exceptions.
    /// </summary>
    Critical = 3,

    /// <summary>
    /// Warning-level exception. Represents expected client-facing failures
    /// (not found, conflict, forbidden) that do not indicate a system defect.
    /// </summary>
    Warning = 4,

    /// <summary>
    /// Error-level exception. Represents domain rule violations that are more
    /// severe than a warning but do not indicate infrastructure failure.
    /// </summary>
    Error = 5
}
