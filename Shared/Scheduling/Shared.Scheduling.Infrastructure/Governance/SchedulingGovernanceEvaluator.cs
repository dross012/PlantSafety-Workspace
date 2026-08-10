// ============================================================================
//  Project:        Shared.Scheduling.Infrastructure
//  Module:         Governance
//  File:           SchedulingGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 14 (Scheduling Modernization) deliverable. Runs
//                  SchedulingGovernanceRules' pure validation against
//                  Scheduling's own contract/due-time/rule-decision shapes
//                  and reports any violation found through the existing
//                  ILogEventWriter/PlantSafetyMetrics primitives -- mirrors
//                  TelemetryGovernanceEvaluator (Phase 8) exactly. This is
//                  the one place a Scheduling governance violation becomes
//                  observable (a LogEvent with EventId
//                  "SCHEDULING_GOVERNANCE_VIOLATION" plus a
//                  SchedulingGovernanceViolationsDetected metric increment),
//                  not a new event type or a new sink.
// ============================================================================
using Shared.Scheduling.Abstractions.Contracts;
using Shared.Scheduling.Abstractions.DueTime;
using Shared.Scheduling.Abstractions.Governance;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Infrastructure.Telemetry;
using Shared.Telemetry.Abstractions.Events;

namespace Shared.Scheduling.Infrastructure.Governance;

/// <summary>Evaluates Scheduling shapes against SchedulingGovernanceRules and reports any violation found.</summary>
public static class SchedulingGovernanceEvaluator
{
    /// <summary>
    /// Validates <paramref name="metadata"/>. If any violation is found, writes one
    /// SCHEDULING_GOVERNANCE_VIOLATION log event and increments
    /// SchedulingMetrics.GovernanceViolationsDetected once per violation. Returns the violation list
    /// (empty if none).
    /// </summary>
    public static IReadOnlyList<string> EvaluateContractMetadataAndReport(
        SchedulingContractMetadata metadata,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
        => EvaluateAndReport(
            SchedulingGovernanceRules.ValidateContractMetadata(metadata),
            "ValidateContractMetadata",
            metadata.CorrelationId,
            occurredAtUtc,
            logEventWriter);

    /// <summary>
    /// Validates every entry in <paramref name="dueTimes"/>. If any violation is found, writes one
    /// SCHEDULING_GOVERNANCE_VIOLATION log event and increments
    /// SchedulingMetrics.GovernanceViolationsDetected once per violation. Returns the violation list
    /// (empty if none).
    /// </summary>
    public static IReadOnlyList<string> EvaluateDueTimesAndReport(
        IReadOnlyList<ScheduledDueTime> dueTimes,
        string correlationId,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(dueTimes);

        var allViolations = new List<string>();
        foreach (ScheduledDueTime dueTime in dueTimes)
        {
            allViolations.AddRange(SchedulingGovernanceRules.ValidateDueTime(dueTime));
        }

        return EvaluateAndReport(allViolations, "ValidateDueTime", correlationId, occurredAtUtc, logEventWriter);
    }

    /// <summary>
    /// Validates <paramref name="decision"/>. If any violation is found, writes one
    /// SCHEDULING_GOVERNANCE_VIOLATION log event and increments
    /// SchedulingMetrics.GovernanceViolationsDetected once per violation. Returns the violation list
    /// (empty if none).
    /// </summary>
    public static IReadOnlyList<string> EvaluateRuleDecisionAndReport(
        SchedulingRuleDecision decision,
        string correlationId,
        DateTime occurredAtUtc,
        ILogEventWriter logEventWriter)
        => EvaluateAndReport(
            SchedulingGovernanceRules.ValidateRuleDecision(decision),
            "ValidateRuleDecision",
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
            SchedulingMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", ruleName));
        }

        logEventWriter.Write(new LogEvent(
            EventId: SchedulingGovernanceEvents.GovernanceViolation,
            Category: LogCategory.Scheduling,
            Severity: LogSeverity.Warning,
            Message: $"Scheduling shape failed governance rule \"{ruleName}\" ({violations.Count} violation(s)).",
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
