// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           IndicatorCategory.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the Scorecards bounded context. Carries Scorecards-specific category metadata and drives downstream workflow, compliance, and reporting behavior per Handbook Rule 2.7.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scorecards.Enums;

/// <summary>Smart Enum representing the governing category taxonomy for Scorecards.</summary>
public sealed class IndicatorCategory : IEquatable<IndicatorCategory>
{
    /// <summary>Lagging Indicators (Reactive Outcomes)</summary>
    public static readonly IndicatorCategory Lagging = new("Lagging", "Lagging Indicators (Reactive Outcomes)");

    /// <summary>Leading Indicators (Proactive Activities)</summary>
    public static readonly IndicatorCategory Leading = new("Leading", "Leading Indicators (Proactive Activities)");

    /// <summary>Cultural & Precursor Metrics</summary>
    public static readonly IndicatorCategory Cultural = new("Cultural", "Cultural & Precursor Metrics");

    /// <summary>All defined IndicatorCategory instances.</summary>
    public static readonly IReadOnlyList<IndicatorCategory> All = [Lagging, Leading, Cultural];

    /// <summary>The default IndicatorCategory instance, used when no explicit category is supplied.</summary>
    public static IndicatorCategory Default => Lagging;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private IndicatorCategory(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a IndicatorCategory instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static IndicatorCategory From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown IndicatorCategory code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a IndicatorCategory instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out IndicatorCategory value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized IndicatorCategory code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(IndicatorCategory? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as IndicatorCategory);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}