/*==========================================================================================================================
    AUTHOR:             David (Ted)
    CREATED:            2026-05-26
    FILE:               ILogger.cs
    DESCRIPTION:        Canonical logging abstraction for all modules. Defines the deterministic, SIEM-safe, strong-typed
                        logging contract required by the Enterprise Architectural Rule Handbook (2026 Edition). This
                        interface is the ONLY logging abstraction permitted in SharedKernel and MUST be implemented by the
                        Infrastructure layer using the Canonical Log Envelope Rule, Canonical Log Level Rule, and
                        SIEM-safe sanitization pipeline.

    SCHEMA VERSION:     2026.1
    EVOLUTION:          Additive-only. No breaking changes permitted. New methods MUST be appended and MUST NOT alter
                        existing semantics.

    NOTES:
    - MUST NOT leak infrastructure-specific logging frameworks (Serilog, ILogger<T>, NLog, etc.).
    - MUST be deterministic, SIEM-safe, and UTC-only.
    - MUST NOT throw exceptions.
    - MUST NOT mutate state.
    - MUST support structured logging with strong-typed fields.
    - MUST follow the Canonical Log Field Dictionary (Part XX).
    - MUST follow the Canonical Log Envelope Rule (Part 2.22).
    - MUST follow the Canonical Log Level Rule (Part 2.20).
    - MUST follow the Canonical Log Redaction & Sanitization Rule (Part 2.23).
    - MUST follow the Canonical Observability Maturity Model (Part 2.37).
    - MUST be safe for API, Domain, Application, Background Jobs, and Eventing pipelines.
    - MUST be used for ALL success and failure logs (Part 2.19).
==========================================================================================================================*/

using System;

namespace PlantSafety.SharedKernel.Abstractions.Logging;

/// <summary>
/// Canonical logging abstraction used across all layers. This interface defines the deterministic, SIEM-safe,
/// strong-typed logging contract required by the Enterprise Architectural Rule Handbook (2026 Edition).
///
/// <para><b>Purpose</b></para>
/// Provides a unified, infrastructure-agnostic logging API that enforces:
/// - Deterministic log shape
/// - UTC-only timestamps
/// - SIEM-safe sanitization
/// - Strong-typed log levels
/// - Canonical field naming
/// - Additive-only evolution
///
/// <para><b>Invariants</b></para>
/// - MUST NOT leak infrastructure logging frameworks.
/// - MUST NOT throw exceptions.
/// - MUST NOT mutate state.
/// - MUST NOT accept null or whitespace messages.
/// - MUST sanitize all exception metadata before logging.
/// - MUST follow the Canonical Log Envelope Rule (Part 2.22).
///
/// <para><b>Observability Requirements</b></para>
/// - MUST include correlation identifiers.
/// - MUST include causation identifiers.
/// - MUST include module identity.
/// - MUST include operation identity.
/// - MUST include UTC timestamp.
/// - MUST include severity level.
/// - MUST include SIEM-safe error metadata when applicable.
///
/// <para><b>Onboarding Notes</b></para>
/// - This interface is intentionally minimal.
/// - All log enrichment occurs in the Infrastructure layer.
/// - All logs MUST be structured and deterministic.
/// - All logs MUST follow the Canonical Log Field Dictionary (Part XX).
/// - All logs MUST be validated by SafetyAnalyzer (SAF001–SAF009).
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Writes a canonical informational log entry.
    /// Used for successful operations, state transitions, and deterministic behavior.
    /// MUST follow the Canonical Success Logging Rule (Part 2.19).
    /// </summary>
    /// <param name="message">A normalized, SIEM-safe message.</param>
    void Info(string message);

    /// <summary>
    /// Writes a canonical warning log entry.
    /// Used for recoverable anomalies, unexpected-but-handled conditions, and soft failures.
    /// MUST follow the Canonical Log Level Rule (Part 2.20).
    /// </summary>
    /// <param name="message">A normalized, SIEM-safe message.</param>
    void Warn(string message);

    /// <summary>
    /// Writes a canonical error log entry.
    /// Used for failures, exceptions, and violations requiring investigation.
    /// MUST sanitize exception metadata according to the Canonical Log Redaction Rule (Part 2.23).
    /// MUST include correlation and causation identifiers.
    /// MUST include UTC timestamp.
    /// </summary>
    /// <param name="message">A normalized, SIEM-safe message describing the failure.</param>
    /// <param name="exception">Optional exception. MUST be sanitized before logging.</param>
    void Error(string message, Exception? exception = null);
}
