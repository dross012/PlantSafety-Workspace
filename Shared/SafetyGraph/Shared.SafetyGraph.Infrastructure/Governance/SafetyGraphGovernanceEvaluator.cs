// ============================================================================
//  Project:        Shared.SafetyGraph.Infrastructure
//  Module:         Governance
//  File:           SafetyGraphGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Runs SafetyGraphGovernanceRules'
//                  pure validation against Safety Graph's own contract/
//                  structure/escalation shapes and reports any violation
//                  found through the existing ILogEventWriter/
//                  PlantSafetyMetrics primitives -- mirrors
//                  SchedulingGovernanceEvaluator/PredictiveGovernanceEvaluator
//                  (Phases 14/15) exactly.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.SafetyGraph.Abstractions.Governance;
using Shared.SafetyGraph.Abstractions.Graph;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.SafetyGraph.Infrastructure.Governance;

/// <summary>Evaluates Safety Graph shapes against SafetyGraphGovernanceRules and reports any violation found.</summary>
public static class SafetyGraphGovernanceEvaluator
{
    /// <summary>
    /// Validates <paramref name="metadata"/>. If any violation is found, writes one
    /// SAFETY_GRAPH_GOVERNANCE_VIOLATION log event and increments
    /// SafetyGraphMetrics.GovernanceViolationsDetected once per violation.
    /// </summary>
    public static IReadOnlyList<string> EvaluateContractMetadataAndReport(
        SafetyGraphMetadata metadata,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
        => EvaluateAndReport(
            SafetyGraphGovernanceRules.ValidateContractMetadata(metadata),
            "ValidateContractMetadata",
            metadata.CorrelationId,
            occurredAtUtc,
            logEventWriter);

    /// <summary>
    /// Validates <paramref name="graph"/>'s structural integrity. If any violation is found, writes one
    /// SAFETY_GRAPH_GOVERNANCE_VIOLATION log event and increments
    /// SafetyGraphMetrics.GovernanceViolationsDetected once per violation.
    /// </summary>
    public static IReadOnlyList<string> EvaluateGraphStructureAndReport(
        SafetyGraphModel graph,
        string correlationId,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
        => EvaluateAndReport(
            SafetyGraphGovernanceRules.ValidateGraphStructure(graph),
            "ValidateGraphStructure",
            correlationId,
            occurredAtUtc,
            logEventWriter);

    /// <summary>
    /// Validates <paramref name="outcome"/>. If any violation is found, writes one
    /// SAFETY_GRAPH_GOVERNANCE_VIOLATION log event and increments
    /// SafetyGraphMetrics.GovernanceViolationsDetected once per violation.
    /// </summary>
    public static IReadOnlyList<string> EvaluateEscalationOutcomeAndReport(
        PredictiveEscalationOutcome outcome,
        string correlationId,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
        => EvaluateAndReport(
            SafetyGraphGovernanceRules.ValidateEscalationOutcome(outcome),
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
            Telemetry.SafetyGraphMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", ruleName));
        }

        logEventWriter.Write(new LogEvent(
            EventId: SafetyGraphGovernanceEvents.GovernanceViolation,
            Category: LogCategory.SafetyGraph,
            Severity: LogSeverity.Warning,
            Message: $"Safety Graph shape failed governance rule \"{ruleName}\" ({violations.Count} violation(s)).",
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
