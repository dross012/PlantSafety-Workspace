// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Governance
//  File:           PredictiveGovernanceRules.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Pure validation over Predictive
//                  Safety's contract metadata and escalation outcomes,
//                  mirroring SchedulingGovernanceRules (Phase 14) exactly:
//                  every rule here is a pure function -- input in, violation
//                  list out, zero side effects.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;

namespace Shared.PredictiveSafety.Abstractions.Governance;

/// <summary>Pure validation rules for Predictive Safety's contract metadata and escalation outcomes.</summary>
public static class PredictiveGovernanceRules
{
    /// <summary>Validates a PredictiveSafetyContractMetadata's shape. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateContractMetadata(PredictiveSafetyContractMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var violations = new List<string>();

        if (metadata.SchemaVersion != PredictiveSafetyContractMetadata.CurrentSchemaVersion)
        {
            violations.Add($"SchemaVersion {metadata.SchemaVersion} does not match the current PredictiveSafetyContractMetadata schema version ({PredictiveSafetyContractMetadata.CurrentSchemaVersion}).");
        }

        if (string.IsNullOrWhiteSpace(metadata.TenantId))
        {
            violations.Add("TenantId is missing or empty -- tenant propagation is broken for this contract.");
        }

        if (string.IsNullOrWhiteSpace(metadata.CorrelationId))
        {
            violations.Add("CorrelationId is missing or empty -- correlation propagation is broken for this contract.");
        }

        if (metadata.IssuedAtUtc == default)
        {
            violations.Add("IssuedAtUtc is unset (default DateTime) -- every contract must carry a real, deterministic-clock-sourced timestamp.");
        }

        return violations;
    }

    /// <summary>Validates a PredictiveEscalationOutcome's internal consistency. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateEscalationOutcome(PredictiveEscalationOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);

        var violations = new List<string>();

        if (outcome.RequiresEscalation && outcome.AppliedRuleCodes.Count == 0)
        {
            violations.Add("RequiresEscalation is true but AppliedRuleCodes is empty -- escalation must always be traceable to at least one applied rule.");
        }

        if (!outcome.RequiresEscalation && outcome.AppliedRuleCodes.Count > 0)
        {
            violations.Add("RequiresEscalation is false but AppliedRuleCodes is non-empty -- an applied rule must always drive escalation.");
        }

        return violations;
    }
}
