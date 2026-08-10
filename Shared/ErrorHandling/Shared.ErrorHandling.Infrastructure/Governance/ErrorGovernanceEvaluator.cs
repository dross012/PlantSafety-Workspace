// ============================================================================
//  Project:        Shared.ErrorHandling.Infrastructure
//  Module:         Governance
//  File:           ErrorGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. Runs ErrorGovernanceRules' pure
//                  validation against an ErrorEnvelope and reports any
//                  violation found through the existing ILogEventWriter/
//                  PlantSafetyMetrics.GovernanceViolationsDetected counter --
//                  same mechanism Phases 8-11 already established.
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.ErrorHandling.Abstractions.Envelope;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.ErrorHandling.Infrastructure.Governance;

/// <summary>Evaluates an ErrorEnvelope against ErrorGovernanceRules and reports any violation found.</summary>
public static class ErrorGovernanceEvaluator
{
    /// <summary>Validates <paramref name="envelope"/> and reports any violation via an ERROR_GOVERNANCE_VIOLATION log event.</summary>
    public static IReadOnlyList<string> EvaluateAndReport(ErrorEnvelope? envelope, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        IReadOnlyList<string> violations = ErrorGovernanceRules.Validate(envelope);
        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string violation in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", violation));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "ERROR_GOVERNANCE_VIOLATION",
            Category: LogCategory.Infrastructure,
            Severity: LogSeverity.Warning,
            Message: $"ErrorEnvelope failed error governance validation ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?> { ["Violations"] = violations },
            CorrelationId: envelope?.CorrelationId ?? "unknown",
            Module: "Error",
            OccurredAtUtc: clock.UtcNow));

        return violations;
    }
}
