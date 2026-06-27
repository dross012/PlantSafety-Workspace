/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-27
    FILE:               ExceptionExtensions.cs
    DESCRIPTION:        Canonical extension helpers for exceptions. Provides SIEM-safe message extraction, root-cause
                        traversal, and canonical formatting for logs and telemetry.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Primitives.Exceptions;

/// <summary>
/// Canonical extension helpers for exceptions.
/// </summary>
public static class ExceptionExtensions
{
    /// <summary>
    /// Returns the deepest inner exception.
    /// </summary>
    public static Exception GetRootCause(this Exception ex)
    {
        while (ex.InnerException != null)
            ex = ex.InnerException;

        return ex;
    }

    /// <summary>
    /// Returns a SIEM-safe message for logging.
    /// </summary>
    public static string ToSiemSafeMessage(this Exception ex)
        => ex.Message?.Trim() ?? "An error occurred.";
}
