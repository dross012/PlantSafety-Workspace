// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyPolicies
//  Layer:          Domain
//  File:           ReviewCadence.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the mandated review/re-approval cadence of a SafetyPolicy record.
//                  Grounded in PlantSafety_Program_Structure_extract.txt Safety Policies Module
//                  (revision history / review requirements referenced across all 17 policy categories).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyPolicies.Enums;

/// <summary>Smart Enum representing how frequently a SafetyPolicy must be formally reviewed.</summary>
public sealed class ReviewCadence : IEquatable<ReviewCadence>
{
    /// <summary>Reviewed every month. Reserved for the highest-risk categories (e.g. Confined Space, Hot Work).</summary>
    public static readonly ReviewCadence Monthly = new("Monthly", "Monthly", 1);

    /// <summary>Reviewed every quarter (3 months).</summary>
    public static readonly ReviewCadence Quarterly = new("Quarterly", "Quarterly", 3);

    /// <summary>Reviewed twice a year (6 months).</summary>
    public static readonly ReviewCadence SemiAnnual = new("SemiAnnual", "Semi-Annual", 6);

    /// <summary>Reviewed once a year. The default cadence for most policy categories.</summary>
    public static readonly ReviewCadence Annual = new("Annual", "Annual", 12);

    /// <summary>Reviewed once every two years. Reserved for low-volatility administrative policies.</summary>
    public static readonly ReviewCadence Biennial = new("Biennial", "Biennial", 24);

    /// <summary>All defined ReviewCadence instances.</summary>
    public static readonly IReadOnlyList<ReviewCadence> All = [Monthly, Quarterly, SemiAnnual, Annual, Biennial];

    /// <summary>The default ReviewCadence instance, used when no explicit cadence is supplied.</summary>
    public static ReviewCadence Default => Annual;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Number of calendar months between mandated reviews.</summary>
    public int IntervalMonths { get; }

    private ReviewCadence(string code, string displayName, int intervalMonths)
    {
        Code = code;
        DisplayName = displayName;
        IntervalMonths = intervalMonths;
    }

    /// <summary>Computes the next review due date from <paramref name="fromUtc"/>.</summary>
    public DateTime ComputeNextDueUtc(DateTime fromUtc) => fromUtc.AddMonths(IntervalMonths);

    /// <summary>Resolves a ReviewCadence instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static ReviewCadence From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown ReviewCadence code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a ReviewCadence instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out ReviewCadence value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized ReviewCadence code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(ReviewCadence? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ReviewCadence);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
