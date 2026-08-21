// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Events
//  File:           UnifiedEventTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. Records the outcome of a
//                  UnifiedEventGovernance check through the existing
//                  ILogEventWriter/PlantSafetyMetrics.GovernanceViolationsDetected
//                  counter -- same reporting shape every governance
//                  evaluator in this program already uses.
// ============================================================================
using Shared.Authorization.Abstractions.Contracts;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Events;

/// <summary>Reports UnifiedEventGovernance violations through Shared.Telemetry's existing primitives.</summary>
public static class UnifiedEventTelemetry
{
    /// <summary>Validates <paramref name="logEvent"/> against <paramref name="metadata"/> and reports any violation via an EVENT_GOVERNANCE_VIOLATION log event.</summary>
    public static IReadOnlyList<string> ValidateAndReport(ILogEvent? logEvent, ContractMetadata metadata, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        IReadOnlyList<string> violations = UnifiedEventGovernance.ValidateCorrelationConsistency(logEvent, metadata);
        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string violation in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", violation));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "EVENT_GOVERNANCE_VIOLATION",
            Category: LogCategory.Infrastructure,
            Severity: LogSeverity.Warning,
            Message: $"LogEvent failed unified event governance validation ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?> { ["Violations"] = violations },
            CorrelationId: metadata.CorrelationId,
            Module: "Event",
            OccurredAtUtc: clock.UtcNow));

        return violations;
    }
}
