// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AccidentManagement
//  Layer:          Domain
//  File:           InvestigationPhase.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the AccidentManagement bounded context. Carries AccidentManagement-specific category metadata and drives downstream workflow, compliance, and reporting behavior per Handbook Rule 2.7.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.AccidentManagement.Enums;

/// <summary>Smart Enum representing the governing category taxonomy for AccidentManagement.</summary>
public sealed class InvestigationPhase : IEquatable<InvestigationPhase>
{
    /// <summary>Site Securing, Injury Triage & Emergency Plan Execution</summary>
    public static readonly InvestigationPhase SiteSecuringAndTriage = new("SiteSecuringAndTriage", "Site Securing, Injury Triage & Emergency Plan Execution");

    /// <summary>Investigation & Root Cause Analysis</summary>
    public static readonly InvestigationPhase InvestigationRootCauseAnalysis = new("InvestigationRootCauseAnalysis", "Investigation & Root Cause Analysis");

    /// <summary>Corrective Actions (CAPA)</summary>
    public static readonly InvestigationPhase CorrectiveActionsCAPA = new("CorrectiveActionsCAPA", "Corrective Actions (CAPA)");

    /// <summary>Regulatory Compliance & Documentation</summary>
    public static readonly InvestigationPhase RegulatoryComplianceDocumentation = new("RegulatoryComplianceDocumentation", "Regulatory Compliance & Documentation");

    /// <summary>All defined InvestigationPhase instances.</summary>
    public static readonly IReadOnlyList<InvestigationPhase> All = [SiteSecuringAndTriage, InvestigationRootCauseAnalysis, CorrectiveActionsCAPA, RegulatoryComplianceDocumentation];

    /// <summary>The default InvestigationPhase instance, used when no explicit category is supplied.</summary>
    public static InvestigationPhase Default => SiteSecuringAndTriage;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private InvestigationPhase(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a InvestigationPhase instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static InvestigationPhase From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown InvestigationPhase code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a InvestigationPhase instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out InvestigationPhase value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized InvestigationPhase code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(InvestigationPhase? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as InvestigationPhase);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}