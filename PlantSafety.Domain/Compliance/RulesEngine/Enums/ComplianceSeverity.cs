// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceSeverity.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the severity of a detected compliance violation. Drives
//                  overall ComplianceEvaluation pass/fail via ComplianceOutcomePolicy.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RulesEngine.Enums;

/// <summary>Smart Enum representing the severity of a detected compliance violation.</summary>
public sealed class ComplianceSeverity : IEquatable<ComplianceSeverity>
{
    /// <summary>Informational only; does not affect overall pass/fail.</summary>
    public static readonly ComplianceSeverity Advisory = new("Advisory", "Advisory", 0);

    /// <summary>Minor violation; tracked but does not fail the evaluation on its own.</summary>
    public static readonly ComplianceSeverity Low = new("Low", "Low", 1);

    /// <summary>Moderate violation; contributes to a failed evaluation when combined with others.</summary>
    public static readonly ComplianceSeverity Medium = new("Medium", "Medium", 2);

    /// <summary>Serious violation; always fails the overall evaluation.</summary>
    public static readonly ComplianceSeverity High = new("High", "High", 3);

    /// <summary>Severe violation requiring immediate remediation; always fails the overall evaluation.</summary>
    public static readonly ComplianceSeverity Critical = new("Critical", "Critical", 4);

    /// <summary>All defined ComplianceSeverity instances, ordered from least to most severe.</summary>
    public static readonly IReadOnlyList<ComplianceSeverity> All = [Advisory, Low, Medium, High, Critical];

    /// <summary>The default ComplianceSeverity instance.</summary>
    public static ComplianceSeverity Default => Low;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Ordinal rank used for deterministic severity comparisons (higher = more severe).</summary>
    public int Rank { get; }

    private ComplianceSeverity(string code, string displayName, int rank)
    {
        Code = code;
        DisplayName = displayName;
        Rank = rank;
    }

    /// <summary>Resolves a ComplianceSeverity instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static ComplianceSeverity From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown ComplianceSeverity code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a ComplianceSeverity instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out ComplianceSeverity value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(ComplianceSeverity? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ComplianceSeverity);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
