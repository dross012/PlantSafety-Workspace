// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           AuthenticationGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. Runs AuthenticationGovernanceRules'
//                  pure validation against a CallerContextResolution and
//                  reports any violation found through the existing
//                  ILogEventWriter/PlantSafetyMetrics.GovernanceViolationsDetected
//                  counter -- the exact same counter and reporting shape
//                  AuthorizationGovernanceEvaluator (Phase 9) and
//                  TelemetryGovernanceEvaluator (Phase 8) already established,
//                  not a third, parallel governance-reporting mechanism.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Evaluates a CallerContextResolution against AuthenticationGovernanceRules and reports any violation found.</summary>
public static class AuthenticationGovernanceEvaluator
{
    /// <summary>
    /// Validates <paramref name="resolution"/>. If any violation is found, writes one
    /// AUTHENTICATION_GOVERNANCE_VIOLATION log event and increments
    /// PlantSafetyMetrics.GovernanceViolationsDetected once per violation. Returns the
    /// violation list (empty if none).
    /// </summary>
    public static IReadOnlyList<string> EvaluateAndReport(CallerContextResolution? resolution, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        IReadOnlyList<string> violations = AuthenticationGovernanceRules.ValidateResolution(resolution);
        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string violation in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", violation));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "AUTHENTICATION_GOVERNANCE_VIOLATION",
            Category: LogCategory.Authentication,
            Severity: LogSeverity.Warning,
            Message: $"CallerContextResolution failed authentication governance validation ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?> { ["Violations"] = violations },
            CorrelationId: resolution?.Caller?.CorrelationId ?? "unknown",
            Module: "Authentication",
            OccurredAtUtc: clock.UtcNow));

        return violations;
    }
}
