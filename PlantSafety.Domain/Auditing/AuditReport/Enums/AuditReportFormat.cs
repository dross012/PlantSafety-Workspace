// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportFormat.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the exportable output formats supported by AuditReport
//                  generation (Handbook Phase 5 §6.2: PDF/CSV/JSON export flows). Additive-only
//                  evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditReport.Enums;

/// <summary>Smart Enum representing a supported AuditReport export format.</summary>
public sealed class AuditReportFormat : IEquatable<AuditReportFormat>
{
    /// <summary>Printable PDF document, rendered via IAuditReportRenderer.</summary>
    public static readonly AuditReportFormat Pdf = new("Pdf", "PDF Document", "application/pdf");

    /// <summary>Comma-separated values export.</summary>
    public static readonly AuditReportFormat Csv = new("Csv", "CSV Export", "text/csv");

    /// <summary>Structured JSON export.</summary>
    public static readonly AuditReportFormat Json = new("Json", "JSON Export", "application/json");

    /// <summary>All defined AuditReportFormat instances.</summary>
    public static readonly IReadOnlyList<AuditReportFormat> All = [Pdf, Csv, Json];

    /// <summary>The default AuditReportFormat instance.</summary>
    public static AuditReportFormat Default => Pdf;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>MIME content type produced when rendering this format.</summary>
    public string ContentType { get; }

    private AuditReportFormat(string code, string displayName, string contentType)
    {
        Code = code;
        DisplayName = displayName;
        ContentType = contentType;
    }

    /// <summary>Resolves an AuditReportFormat instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static AuditReportFormat From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown AuditReportFormat code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve an AuditReportFormat instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out AuditReportFormat value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(AuditReportFormat? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as AuditReportFormat);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
