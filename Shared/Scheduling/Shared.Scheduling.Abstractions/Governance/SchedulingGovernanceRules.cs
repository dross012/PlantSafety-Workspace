// ============================================================================
//  Project:        Shared.Scheduling.Abstractions
//  Module:         Governance
//  File:           SchedulingGovernanceRules.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Pure
//                  validation over Scheduling's own contract/schedule
//                  shapes, mirroring TelemetryGovernanceRules (Phase 8)
//                  exactly: every rule here is a pure function -- input in,
//                  violation list out, zero side effects. Violations are
//                  reported through the caller (SchedulingGovernanceEvaluator,
//                  Shared.Scheduling.Infrastructure) as an ordinary LogEvent
//                  and a PlantSafetyMetrics counter increment, not a new
//                  event type or sink.
// ============================================================================
using Shared.Scheduling.Abstractions.Contracts;
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Rules;

namespace Shared.Scheduling.Abstractions.Governance;

/// <summary>Pure validation rules for Scheduling's contract metadata, due times, and rule decisions.</summary>
public static class SchedulingGovernanceRules
{
    /// <summary>Validates a SchedulingContractMetadata's shape. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateContractMetadata(SchedulingContractMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        var violations = new List<string>();

        if (metadata.SchemaVersion != SchedulingContractMetadata.CurrentSchemaVersion)
        {
            violations.Add($"SchemaVersion {metadata.SchemaVersion} does not match the current SchedulingContractMetadata schema version ({SchedulingContractMetadata.CurrentSchemaVersion}).");
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

    /// <summary>Validates a single ScheduledDueTime's shape. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateDueTime(ScheduledDueTime dueTime)
    {
        ArgumentNullException.ThrowIfNull(dueTime);

        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(dueTime.ActionCode))
        {
            violations.Add("ActionCode is missing or empty.");
        }

        if (dueTime.DueAtUtc == default)
        {
            violations.Add("DueAtUtc is unset (default DateTime) -- every due time must be derived from a real asOfUtc.");
        }

        return violations;
    }

    /// <summary>Validates a SchedulingRuleDecision's internal consistency. Returns every violation found (empty if valid).</summary>
    public static IReadOnlyList<string> ValidateRuleDecision(SchedulingRuleDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        var violations = new List<string>();

        if (decision.TotalRequiredActionCount < 0)
        {
            violations.Add("TotalRequiredActionCount is negative -- additive accumulation must never go below zero.");
        }

        if (decision.TotalEstimatedEffortHours < 0)
        {
            violations.Add("TotalEstimatedEffortHours is negative -- additive accumulation must never go below zero.");
        }

        if (decision.TotalRequiredActionCount == 0 && decision.AppliedRuleCodes.Count > 0)
        {
            violations.Add("AppliedRuleCodes is non-empty despite TotalRequiredActionCount being zero -- no rule should apply to an empty evaluation.");
        }

        return violations;
    }
}
