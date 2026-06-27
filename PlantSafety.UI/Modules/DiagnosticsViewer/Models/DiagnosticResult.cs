// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Client-side materialized DiagnosticResult DTO. Represents
//   a single SafetyAnalyzer rule violation. The UI renders materialized results
//   only — it never performs raw rule evaluation. Ordered: Severity → Category
//   → Rule → Target per Part XI-A rendering rules.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI-A
// ============================================================

using System;

namespace PlantSafety.UI.Modules.DiagnosticsViewer.Models;

/// <summary>Canonical severity levels for diagnostic results. Ordered by descending severity.</summary>
public enum DiagnosticSeverity
{
    /// <summary>Blocks build or compile; must be resolved before release.</summary>
    Error = 0,

    /// <summary>Structural or risky pattern; should be resolved before release.</summary>
    Warning = 1,

    /// <summary>Advisory finding; non-blocking.</summary>
    Information = 2
}

/// <summary>
/// Immutable client-side representation of a single SafetyAnalyzer diagnostic finding.
/// Must NOT be mutated after deserialization — the UI renders the materialized object as-is.
/// </summary>
public sealed class DiagnosticResult
{
    /// <summary>Gets the unique identifier for this diagnostic result (UUIDv7).</summary>
    public Guid ResultId { get; init; }

    /// <summary>Gets the SafetyAnalyzer rule code (e.g., "SAF001").</summary>
    public string RuleCode { get; init; } = string.Empty;

    /// <summary>Gets the severity of this finding.</summary>
    public DiagnosticSeverity Severity { get; init; }

    /// <summary>Gets the category of this finding (e.g., "ForbiddenArtifact", "ModuleBoundary").</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the target file path relative to the solution root.</summary>
    public string Target { get; init; } = string.Empty;

    /// <summary>Gets the one-based line number within the target file, or null for file-level findings.</summary>
    public int? LineNumber { get; init; }

    /// <summary>Gets the human-readable diagnostic message. Never contains PII or stack traces.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Gets the handbook section reference for this rule (e.g., "§5.9").</summary>
    public string HandbookSection { get; init; } = string.Empty;

    /// <summary>Gets a suggested fix description. Empty when no automated fix is available.</summary>
    public string SuggestedFix { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether an automated SafetyCLI fix is available.</summary>
    public bool HasAutoFix { get; init; }

    /// <summary>Gets the UTC timestamp when this finding was recorded.</summary>
    public DateTimeOffset TimestampUtc { get; init; }

    /// <summary>Gets the correlation identifier of the diagnostic run that produced this result.</summary>
    public Guid RunCorrelationId { get; init; }
}
