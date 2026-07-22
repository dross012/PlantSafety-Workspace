// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskLevel.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the RiskManagement bounded context. Carries RiskManagement-specific category metadata and drives downstream workflow, compliance, and reporting behavior per Handbook Rule 2.7.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.RiskManagement.Enums;

/// <summary>Smart Enum representing the governing category taxonomy for RiskManagement.</summary>
public sealed class RiskLevel : IEquatable<RiskLevel>
{
    /// <summary>Low Risk</summary>
    public static readonly RiskLevel Low = new("Low", "Low Risk");

    /// <summary>Medium Risk</summary>
    public static readonly RiskLevel Medium = new("Medium", "Medium Risk");

    /// <summary>High Risk</summary>
    public static readonly RiskLevel High = new("High", "High Risk");

    /// <summary>Critical Risk</summary>
    public static readonly RiskLevel Critical = new("Critical", "Critical Risk");

    /// <summary>All defined RiskLevel instances.</summary>
    public static readonly IReadOnlyList<RiskLevel> All = [Low, Medium, High, Critical];

    /// <summary>The default RiskLevel instance, used when no explicit category is supplied.</summary>
    public static RiskLevel Default => Low;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private RiskLevel(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a RiskLevel instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static RiskLevel From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown RiskLevel code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a RiskLevel instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out RiskLevel value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized RiskLevel code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(RiskLevel? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as RiskLevel);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}