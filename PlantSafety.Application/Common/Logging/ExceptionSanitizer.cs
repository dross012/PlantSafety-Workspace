// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Canonical exception sanitization utility (Handbook §2.23 Canonical Log
//   Redaction & Sanitization Rule, §2.30 Canonical Logging Code Examples). Produces a
//   SIEM-safe, hashed projection of an exception -- never the raw stack trace, connection
//   string, or SQL text -- suitable for embedding in a structured error log envelope.
// SchemaVersion: 2026.1
// Additive-only evolution: new fields may be added; no existing member may be removed or renamed.

using System.Security.Cryptography;
using System.Text;

namespace PlantSafety.Application.Common.Logging;

/// <summary>
/// Sanitizes exceptions into a SIEM-safe, structured, hashed projection per the Canonical Log
/// Redaction &amp; Sanitization Rule (§2.23). Never exposes raw stack traces, SQL text, or
/// connection strings -- only stable hashes and classification metadata.
/// </summary>
public static class ExceptionSanitizer
{
    /// <summary>Sanitizes <paramref name="exception"/> into a <see cref="SanitizedException"/> projection.</summary>
    /// <param name="exception">The exception to sanitize.</param>
    /// <returns>A SIEM-safe, hashed projection of the exception.</returns>
    public static SanitizedException Sanitize(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return new SanitizedException(
            ExceptionType: exception.GetType().Name,
            Message: exception.Message,
            HResult: exception.HResult,
            StackTraceHash: ComputeHash(exception.StackTrace),
            InnerExceptionType: exception.InnerException?.GetType().Name,
            InnerMessage: exception.InnerException?.Message,
            Severity: exception is OperationCanceledException ? "Warning" : "Error",
            IsTransient: exception is TimeoutException or TaskCanceledException,
            IsDomainSafe: exception is not (OutOfMemoryException or StackOverflowException or AccessViolationException));
    }

    private static string? ComputeHash(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash)[..16];
    }
}

/// <summary>
/// SIEM-safe, hashed projection of an exception. Never contains raw stack traces, SQL text,
/// connection strings, or PII -- only sanitized/hashed metadata per §2.23.
/// </summary>
/// <param name="ExceptionType">The exception's CLR type name.</param>
/// <param name="Message">The exception message (assumed free of secrets/PII at the throw site).</param>
/// <param name="HResult">The exception's HRESULT.</param>
/// <param name="StackTraceHash">Stable SHA-256-derived hash of the raw stack trace. Never the raw trace itself.</param>
/// <param name="InnerExceptionType">The inner exception's CLR type name, if any.</param>
/// <param name="InnerMessage">The inner exception's message, if any.</param>
/// <param name="Severity">Canonical severity classification ("Warning" or "Error").</param>
/// <param name="IsTransient">True when the failure is likely transient (timeout/cancellation) and safe to retry.</param>
/// <param name="IsDomainSafe">True unless the exception is a fatal runtime failure (OOM/stack overflow/access violation).</param>
public sealed record SanitizedException(
    string ExceptionType,
    string Message,
    int HResult,
    string? StackTraceHash,
    string? InnerExceptionType,
    string? InnerMessage,
    string Severity,
    bool IsTransient,
    bool IsDomainSafe);
