// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Events
//  File:           LogSeverity.cs
//  Created:        2026-08-09
//  Description:    Phase 2.4 (Logging & Telemetry Normalization).
// ============================================================================
namespace Shared.Telemetry.Abstractions.Events;

/// <summary>The severity of a structured log event.</summary>
public enum LogSeverity
{
    /// <summary>Fine-grained diagnostic detail, off by default in production.</summary>
    Trace,

    /// <summary>Diagnostic detail useful while developing/troubleshooting.</summary>
    Debug,

    /// <summary>Normal, expected operational activity.</summary>
    Info,

    /// <summary>An unexpected but non-fatal condition.</summary>
    Warning,

    /// <summary>An operation failed.</summary>
    Error,

    /// <summary>A failure severe enough to threaten the whole application.</summary>
    Critical,
}
