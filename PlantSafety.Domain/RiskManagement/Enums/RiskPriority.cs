// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         RiskManagement
//  Layer:          Domain
//  File:           RiskPriority.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for risk remediation priority, taken verbatim from Full Risk
//                  Management Process, PHASE 1 Step 3 ("Prioritize Risks"): "P1 Critical
//                  (Immediate action required), P2 High (Action within 24-72 hours), P3 Medium
//                  (Action within 7-30 days), P4 Low (Monitor or schedule during routine
//                  maintenance)".
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.RiskManagement.Enums;

/// <summary>Smart Enum representing the P1-P4 remediation priority assigned during risk prioritization.</summary>
public sealed class RiskPriority : IEquatable<RiskPriority>
{
    /// <summary>P1 - Critical. Immediate action required.</summary>
    public static readonly RiskPriority P1Critical = new("P1", "Critical", "Immediate action required");

    /// <summary>P2 - High. Action within 24-72 hours.</summary>
    public static readonly RiskPriority P2High = new("P2", "High", "Action within 24-72 hours");

    /// <summary>P3 - Medium. Action within 7-30 days.</summary>
    public static readonly RiskPriority P3Medium = new("P3", "Medium", "Action within 7-30 days");

    /// <summary>P4 - Low. Monitor or schedule during routine maintenance.</summary>
    public static readonly RiskPriority P4Low = new("P4", "Low", "Monitor or schedule during routine maintenance");

    /// <summary>All defined RiskPriority instances.</summary>
    public static readonly IReadOnlyList<RiskPriority> All = [P1Critical, P2High, P3Medium, P4Low];

    /// <summary>The default RiskPriority instance, used when no explicit priority is supplied.</summary>
    public static RiskPriority Default => P4Low;

    /// <summary>Stable machine-readable code (P1..P4).</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Expected response window, as documented in the Full Risk Management Process.</summary>
    public string ResponseWindow { get; }

    private RiskPriority(string code, string displayName, string responseWindow)
    {
        Code = code;
        DisplayName = displayName;
        ResponseWindow = responseWindow;
    }

    /// <summary>Derives the recommended priority from a computed risk level, per the standard escalation mapping.</summary>
    public static RiskPriority FromRiskLevel(RiskLevel level)
        => level.Code switch
        {
            "Critical" => P1Critical,
            "High" => P2High,
            "Medium" => P3Medium,
            _ => P4Low,
        };

    /// <summary>Resolves a RiskPriority instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static RiskPriority From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown RiskPriority code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a RiskPriority instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out RiskPriority value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized RiskPriority code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(RiskPriority? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as RiskPriority);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
