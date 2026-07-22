// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskReviewCadence.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the periodic review cadence of a Mitigated risk, taken verbatim
//                  from Full Risk Management Process PHASE 3 Step 10 ("Review & Improve"):
//                  "Monthly risk review meetings, Quarterly safety performance evaluations,
//                  Annual risk management program audits".
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.RiskManagement.Enums;

/// <summary>Smart Enum representing how frequently a Mitigated Risk must be formally re-reviewed.</summary>
public sealed class RiskReviewCadence : IEquatable<RiskReviewCadence>
{
    /// <summary>Monthly risk review meetings.</summary>
    public static readonly RiskReviewCadence Monthly = new("Monthly", "Monthly Risk Review Meeting", 1);

    /// <summary>Quarterly safety performance evaluations.</summary>
    public static readonly RiskReviewCadence Quarterly = new("Quarterly", "Quarterly Safety Performance Evaluation", 3);

    /// <summary>Annual risk management program audits.</summary>
    public static readonly RiskReviewCadence Annual = new("Annual", "Annual Risk Management Program Audit", 12);

    /// <summary>All defined RiskReviewCadence instances.</summary>
    public static readonly IReadOnlyList<RiskReviewCadence> All = [Monthly, Quarterly, Annual];

    /// <summary>The default RiskReviewCadence instance.</summary>
    public static RiskReviewCadence Default => Monthly;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Number of calendar months between mandated reviews.</summary>
    public int IntervalMonths { get; }

    private RiskReviewCadence(string code, string displayName, int intervalMonths)
    {
        Code = code;
        DisplayName = displayName;
        IntervalMonths = intervalMonths;
    }

    /// <summary>Computes the next review due date from <paramref name="fromUtc"/>.</summary>
    public DateTime ComputeNextDueUtc(DateTime fromUtc) => fromUtc.AddMonths(IntervalMonths);

    /// <summary>Resolves a RiskReviewCadence instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static RiskReviewCadence From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown RiskReviewCadence code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a RiskReviewCadence instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out RiskReviewCadence value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized RiskReviewCadence code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(RiskReviewCadence? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as RiskReviewCadence);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
