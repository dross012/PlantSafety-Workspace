// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           ScorecardPeriodType.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the reporting period a Scorecard covers, grounded in
//                  "Injury charts (monthly, quarterly, annual)" (Lagging Indicator Panel) and
//                  "Included in monthly safety meetings... Integrated into annual safety
//                  performance reports" (Reporting & Distribution).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scorecards.Enums;

/// <summary>Smart Enum representing the reporting period cadence a Scorecard covers.</summary>
public sealed class ScorecardPeriodType : IEquatable<ScorecardPeriodType>
{
    /// <summary>Monthly scorecard, e.g. for monthly safety meetings.</summary>
    public static readonly ScorecardPeriodType Monthly = new("Monthly", "Monthly", 1);

    /// <summary>Quarterly scorecard.</summary>
    public static readonly ScorecardPeriodType Quarterly = new("Quarterly", "Quarterly", 3);

    /// <summary>Annual scorecard, e.g. for annual safety performance reports.</summary>
    public static readonly ScorecardPeriodType Annual = new("Annual", "Annual", 12);

    /// <summary>All defined ScorecardPeriodType instances.</summary>
    public static readonly IReadOnlyList<ScorecardPeriodType> All = [Monthly, Quarterly, Annual];

    /// <summary>The default ScorecardPeriodType instance.</summary>
    public static ScorecardPeriodType Default => Monthly;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Number of calendar months the period spans.</summary>
    public int IntervalMonths { get; }

    private ScorecardPeriodType(string code, string displayName, int intervalMonths)
    {
        Code = code;
        DisplayName = displayName;
        IntervalMonths = intervalMonths;
    }

    /// <summary>Computes the period end (exclusive) given a period start.</summary>
    public DateTime ComputePeriodEndUtc(DateTime periodStartUtc) => periodStartUtc.AddMonths(IntervalMonths);

    /// <summary>Resolves a ScorecardPeriodType instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static ScorecardPeriodType From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown ScorecardPeriodType code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a ScorecardPeriodType instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out ScorecardPeriodType value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized ScorecardPeriodType code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(ScorecardPeriodType? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ScorecardPeriodType);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
