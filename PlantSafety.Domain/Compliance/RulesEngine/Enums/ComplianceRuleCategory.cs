// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RulesEngine
//  Layer:          Domain
//  File:           ComplianceRuleCategory.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the three compliance-rule pipelines that make up the
//                  Compliance Rules Engine (Handbook Phase 5 §6.3): Safety, Regulatory, and
//                  InternalPolicy. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Compliance.RulesEngine.Enums;

/// <summary>Smart Enum categorizing a compliance rule's governing pipeline.</summary>
public sealed class ComplianceRuleCategory : IEquatable<ComplianceRuleCategory>
{
    /// <summary>Safety-compliance rules (hazard controls, permits, PPE, incident follow-up).</summary>
    public static readonly ComplianceRuleCategory Safety = new("Safety", "Safety Compliance");

    /// <summary>Regulatory-compliance rules (statutory obligations, certifications, permits).</summary>
    public static readonly ComplianceRuleCategory Regulatory = new("Regulatory", "Regulatory Compliance");

    /// <summary>Internal-policy compliance rules (company procedures, MoC approvals, acknowledgments).</summary>
    public static readonly ComplianceRuleCategory InternalPolicy = new("InternalPolicy", "Internal Policy Compliance");

    /// <summary>All defined ComplianceRuleCategory instances.</summary>
    public static readonly IReadOnlyList<ComplianceRuleCategory> All = [Safety, Regulatory, InternalPolicy];

    /// <summary>The default ComplianceRuleCategory instance.</summary>
    public static ComplianceRuleCategory Default => Safety;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private ComplianceRuleCategory(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a ComplianceRuleCategory instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static ComplianceRuleCategory From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown ComplianceRuleCategory code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a ComplianceRuleCategory instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out ComplianceRuleCategory value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(ComplianceRuleCategory? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ComplianceRuleCategory);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
