// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportStatus.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the lifecycle status of an AuditReport aggregate.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditReport.Enums;

/// <summary>Smart Enum representing the lifecycle status of an AuditReport.</summary>
public sealed class AuditReportStatus : IEquatable<AuditReportStatus>
{
    /// <summary>Report content has been composed but not yet rendered to its target format.</summary>
    public static readonly AuditReportStatus Draft = new("Draft", "Draft");

    /// <summary>Report was rendered successfully and is available for download.</summary>
    public static readonly AuditReportStatus Generated = new("Generated", "Generated");

    /// <summary>Report generation failed.</summary>
    public static readonly AuditReportStatus Failed = new("Failed", "Failed");

    /// <summary>All defined AuditReportStatus instances.</summary>
    public static readonly IReadOnlyList<AuditReportStatus> All = [Draft, Generated, Failed];

    /// <summary>The default AuditReportStatus instance.</summary>
    public static AuditReportStatus Default => Draft;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private AuditReportStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves an AuditReportStatus instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static AuditReportStatus From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown AuditReportStatus code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve an AuditReportStatus instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out AuditReportStatus value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(AuditReportStatus? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as AuditReportStatus);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
