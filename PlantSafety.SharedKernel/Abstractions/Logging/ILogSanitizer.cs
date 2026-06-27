/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ILogSanitizer.cs
    DESCRIPTION:        Abstraction for SIEM-safe sanitization of log messages and exception metadata. Enforces the
                        Canonical Log Redaction & Sanitization Rule and No PII Rule.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only.
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Abstractions.Logging;

/// <summary>
/// Sanitizes log messages and exception metadata to ensure SIEM-safe, PII-free logging.
/// </summary>
public interface ILogSanitizer
{
    string SanitizeMessage(string message);
    Exception? SanitizeException(Exception? exception);
}
