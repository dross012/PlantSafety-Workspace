// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportSectionType.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the canonical compliance-formatted sections of a printable
//                  audit report (Handbook Phase 5 §6.2): Identity, Timeline, Approvals,
//                  Exceptions, Signatures. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditReport.Enums;

/// <summary>Smart Enum representing a canonical section of a printable audit report.</summary>
public sealed class AuditReportSectionType : IEquatable<AuditReportSectionType>
{
    /// <summary>Entity identity section (who/what/where being audited).</summary>
    public static readonly AuditReportSectionType EntityIdentity = new("EntityIdentity", "Entity Identity", 1);

    /// <summary>Chronological AuditTimeline section (events, state changes, conflicts, scheduling decisions).</summary>
    public static readonly AuditReportSectionType Timeline = new("Timeline", "Timeline", 2);

    /// <summary>Approvals section (who approved/reviewed, and when).</summary>
    public static readonly AuditReportSectionType Approvals = new("Approvals", "Approvals", 3);

    /// <summary>Exceptions section (compliance violations and unresolved findings).</summary>
    public static readonly AuditReportSectionType Exceptions = new("Exceptions", "Exceptions", 4);

    /// <summary>Signatures/attestations section.</summary>
    public static readonly AuditReportSectionType Signatures = new("Signatures", "Signatures & Attestations", 5);

    /// <summary>All defined AuditReportSectionType instances, in canonical print order.</summary>
    public static readonly IReadOnlyList<AuditReportSectionType> All = [EntityIdentity, Timeline, Approvals, Exceptions, Signatures];

    /// <summary>The default AuditReportSectionType instance.</summary>
    public static AuditReportSectionType Default => EntityIdentity;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Canonical print order (lower prints first).</summary>
    public int PrintOrder { get; }

    private AuditReportSectionType(string code, string displayName, int printOrder)
    {
        Code = code;
        DisplayName = displayName;
        PrintOrder = printOrder;
    }

    /// <summary>Resolves an AuditReportSectionType instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static AuditReportSectionType From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown AuditReportSectionType code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve an AuditReportSectionType instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out AuditReportSectionType value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(AuditReportSectionType? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as AuditReportSectionType);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
