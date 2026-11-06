// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           ContractGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. Runs ContractValidationRules' pure
//                  validation against a ContractMetadata and reports any
//                  violation found through the existing ILogEventWriter/
//                  PlantSafetyMetrics.GovernanceViolationsDetected counter --
//                  same mechanism Phases 8-11 already established.
// ============================================================================
using Shared.Authorization.Abstractions.Contracts;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Evaluates a ContractMetadata against ContractValidationRules and reports any violation found.</summary>
public static class ContractGovernanceEvaluator
{
    /// <summary>Validates <paramref name="metadata"/> and reports any violation via a CONTRACT_GOVERNANCE_VIOLATION log event.</summary>
    public static IReadOnlyList<string> EvaluateAndReport(ContractMetadata? metadata, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        IReadOnlyList<string> violations = ContractValidationRules.Validate(metadata);
        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string violation in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", violation));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "CONTRACT_GOVERNANCE_VIOLATION",
            Category: LogCategory.Infrastructure,
            Severity: LogSeverity.Warning,
            Message: $"ContractMetadata failed contract governance validation ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?> { ["Violations"] = violations },
            CorrelationId: metadata?.CorrelationId ?? "unknown",
            Module: "Contract",
            OccurredAtUtc: clock.UtcNow));

        return violations;
    }
}
