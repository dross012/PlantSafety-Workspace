// ============================================================================
//  Project:        AI.RegulatoryInterpretation
//  Module:         Abstractions.Rules
//  File:           RegulatoryEscalationRule.cs
//  Created:        2026-08-09
//  Description:    A deterministic rule contributing an escalation note
//                  based on the hazard/severity context and which
//                  jurisdictions RegulatoryInferenceRules (Phase C.4)
//                  found applicable. Distinct from RegulatoryInterpretationRule
//                  (which is per-jurisdiction, catalog-driven guidance);
//                  these rules react to the interpretation as a whole.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;

namespace AI.RegulatoryInterpretation.Abstractions.Rules;

/// <summary>A single deterministic rule contributing an escalation note to a regulatory interpretation.</summary>
public sealed record RegulatoryEscalationRule(
    string RuleId,
    string Name,
    Func<SeverityLevel, IReadOnlyList<RegulatoryJurisdiction>, bool> Condition,
    string Note);

/// <summary>The standard, deterministic escalation rule set.</summary>
public static class RegulatoryInterpretationRuleSet
{
    /// <summary>The standard rules.</summary>
    public static IReadOnlyList<RegulatoryEscalationRule> Standard { get; } =
    [
        new RegulatoryEscalationRule(
            "REG-001",
            "Multi-jurisdiction reporting",
            (_, jurisdictions) => jurisdictions.Count >= 3,
            "Three or more jurisdictions apply; coordinate a single incident timeline across all regulatory filings to keep facts consistent."),

        new RegulatoryEscalationRule(
            "REG-002",
            "Federal and state both apply",
            (_, jurisdictions) => jurisdictions.Contains(RegulatoryJurisdiction.OSHA) && jurisdictions.Contains(RegulatoryJurisdiction.State),
            "Both federal (OSHA) and state jurisdictions apply; state requirements may be stricter and should be checked first."),

        new RegulatoryEscalationRule(
            "REG-003",
            "Catastrophic severity reporting",
            (severity, _) => severity == SeverityLevel.Catastrophic,
            "Catastrophic severity: use the shortest applicable reporting window across all jurisdictions, not each one's typical window."),

        new RegulatoryEscalationRule(
            "REG-004",
            "Environmental jurisdiction present",
            (_, jurisdictions) => jurisdictions.Contains(RegulatoryJurisdiction.EPA),
            "EPA jurisdiction applies; preserve environmental sampling data before remediation begins."),
    ];
}
