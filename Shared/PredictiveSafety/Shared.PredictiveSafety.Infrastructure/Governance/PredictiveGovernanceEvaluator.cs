// ============================================================================
//  Project:        Shared.PredictiveSafety.Infrastructure
//  Module:         Governance
//  File:           PredictiveGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. Runs PredictiveGovernanceRules' pure
//                  validation against Predictive Safety's own contract/
//                  escalation shapes and reports any violation found through
//                  the existing ILogEventWriter/PlantSafetyMetrics
//                  primitives -- mirrors SchedulingGovernanceEvaluator
//                  (Phase 14) exactly.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Abstractions.Governance;
using Shared.PredictiveSafety.Infrastructure.Telemetry;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.PredictiveSafety.Infrastructure.Governance;

/// <summary>Evaluates Predictive Safety shapes against PredictiveGovernanceRules and reports any violation found.</summary>
public static class PredictiveGovernanceEvaluator
{
    /// <summary>
    /// Validates <paramref name="metadata"/>. If any violation is found, writes one
    /// PREDICTIVE_GOVERNANCE_VIOLATION log event and increments
    /// PredictiveMetrics.GovernanceViolationsDetected once per violation. Returns the violation list
    /// (empty if none).
    /// </summary>
    public static IReadOnlyList<string> EvaluateContractMetadataAndReport(
        PredictiveSafetyContractMetadata metadata,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
        => EvaluateAndReport(
            PredictiveGovernanceRules.ValidateContractMetadata(metadata),
            "ValidateContractMetadata",
            metadata.CorrelationId,
            occurredAtUtc,
            logEventWriter);

    /// <summary>
    /// Validates <paramref name="outcome"/>. If any violation is found, writes one
    /// PREDICTIVE_GOVERNANCE_VIOLATION log event and increments
    /// PredictiveMetrics.GovernanceViolationsDetected once per violation. Returns the violation list
    /// (empty if none).
    /// </summary>
    public static IReadOnlyList<string> EvaluateEscalationOutcomeAndReport(
        PredictiveEscalationOutcome outcome,
        string correlationId,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
        => EvaluateAndReport(
            PredictiveGovernanceRules.ValidateEscalationOutcome(outcome),
            "ValidateEscalationOutcome",
            correlationId,
            occurredAtUtc,
            logEventWriter);

    private static IReadOnlyList<string> EvaluateAndReport(
        IReadOnlyList<string> violations,
        string ruleName,
        string correlationId,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(violations);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string _ in violations)
        {
            PredictiveMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", ruleName));
        }

        logEventWriter.Write(new LogEvent(
            EventId: PredictiveGovernanceEvents.GovernanceViolation,
            Category: LogCategory.PredictiveSafety,
            Severity: LogSeverity.Warning,
            Message: $"Predictive Safety shape failed governance rule \"{ruleName}\" ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?>
            {
                ["Rule"] = ruleName,
                ["Violations"] = violations,
            },
            CorrelationId: string.IsNullOrWhiteSpace(correlationId) ? "unknown" : correlationId,
            Module: "Governance",
            OccurredAtUtc: occurredAtUtc));

        return violations;
    }
}
