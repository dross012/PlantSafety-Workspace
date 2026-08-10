// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         Governance
//  File:           SimulationGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. Runs SimulationGovernanceRules'
//                  pure validation against Simulation's own contract/
//                  scenario/escalation shapes and reports any violation
//                  found through the existing ILogEventWriter/
//                  PlantSafetyMetrics primitives -- mirrors
//                  SafetyGraphGovernanceEvaluator (Phase 16) exactly.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.Simulation.Abstractions.Governance;
using Shared.Simulation.Abstractions.Simulation;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Simulation.Infrastructure.Governance;

/// <summary>Evaluates Simulation shapes against SimulationGovernanceRules and reports any violation found.</summary>
public static class SimulationGovernanceEvaluator
{
    /// <summary>
    /// Validates <paramref name="metadata"/>. If any violation is found, writes one
    /// SIMULATION_GOVERNANCE_VIOLATION log event and increments
    /// SimulationMetrics.GovernanceViolationsDetected once per violation.
    /// </summary>
    public static IReadOnlyList<string> EvaluateContractMetadataAndReport(
        SimulationMetadata metadata,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
        => EvaluateAndReport(
            SimulationGovernanceRules.ValidateContractMetadata(metadata),
            "ValidateContractMetadata",
            metadata.CorrelationId,
            occurredAtUtc,
            logEventWriter);

    /// <summary>
    /// Validates <paramref name="scenario"/>. If any violation is found, writes one
    /// SIMULATION_GOVERNANCE_VIOLATION log event and increments
    /// SimulationMetrics.GovernanceViolationsDetected once per violation.
    /// </summary>
    public static IReadOnlyList<string> EvaluateScenarioAndReport(
        SimulationScenario scenario,
        string correlationId,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
        => EvaluateAndReport(
            SimulationGovernanceRules.ValidateScenario(scenario),
            "ValidateScenario",
            correlationId,
            occurredAtUtc,
            logEventWriter);

    /// <summary>
    /// Validates <paramref name="outcome"/>. If any violation is found, writes one
    /// SIMULATION_GOVERNANCE_VIOLATION log event and increments
    /// SimulationMetrics.GovernanceViolationsDetected once per violation.
    /// </summary>
    public static IReadOnlyList<string> EvaluateEscalationOutcomeAndReport(
        PredictiveEscalationOutcome outcome,
        string correlationId,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
        => EvaluateAndReport(
            SimulationGovernanceRules.ValidateEscalationOutcome(outcome),
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
            Telemetry.SimulationMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", ruleName));
        }

        logEventWriter.Write(new LogEvent(
            EventId: SimulationGovernanceEvents.GovernanceViolation,
            Category: LogCategory.Simulation,
            Severity: LogSeverity.Warning,
            Message: $"Simulation shape failed governance rule \"{ruleName}\" ({violations.Count} violation(s)).",
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
