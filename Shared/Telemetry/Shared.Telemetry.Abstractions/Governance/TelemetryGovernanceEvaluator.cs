// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Governance
//  File:           TelemetryGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 8 deliverable. Runs TelemetryGovernanceRules'
//                  pure validation against a log event and reports any
//                  violation found through the existing ILogEventWriter/
//                  PlantSafetyMetrics primitives -- this is the one place
//                  a violation becomes observable (a LogEvent with
//                  EventId "TELEMETRY_GOVERNANCE_VIOLATION" plus a
//                  GovernanceViolationsDetected metric increment), not a
//                  new event type or a new sink.
// ============================================================================
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Telemetry.Abstractions.Governance;

/// <summary>Evaluates a log event against TelemetryGovernanceRules and reports any violation found.</summary>
public static class TelemetryGovernanceEvaluator
{
    /// <summary>
    /// Validates <paramref name="logEvent"/>. If any violation is found, writes one
    /// TELEMETRY_GOVERNANCE_VIOLATION log event (via <paramref name="logEventWriter"/>) and increments
    /// PlantSafetyMetrics.GovernanceViolationsDetected once per violation. Returns the violation list
    /// (empty if none) so a caller may also choose to act on it directly.
    /// </summary>
    public static IReadOnlyList<string> EvaluateAndReport(ILogEvent logEvent, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        IReadOnlyList<string> violations = TelemetryGovernanceRules.ValidateLogEvent(logEvent);
        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string violation in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", "ValidateLogEvent"));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "TELEMETRY_GOVERNANCE_VIOLATION",
            Category: LogCategory.Infrastructure,
            Severity: LogSeverity.Warning,
            Message: $"Log event \"{logEvent.EventId}\" from module \"{logEvent.Module}\" failed telemetry governance validation ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?>
            {
                ["OffendingEventId"] = logEvent.EventId,
                ["OffendingModule"] = logEvent.Module,
                ["Violations"] = violations,
            },
            CorrelationId: string.IsNullOrWhiteSpace(logEvent.CorrelationId) ? "unknown" : logEvent.CorrelationId,
            Module: "Governance",
            OccurredAtUtc: logEvent.OccurredAtUtc));

        return violations;
    }
}
