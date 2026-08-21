// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           AuthorizationGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. Runs AuthorizationGovernanceRules'
//                  pure validation against a CallerContext and reports any
//                  violation found through the existing ILogEventWriter/
//                  PlantSafetyMetrics.GovernanceViolationsDetected counter
//                  (Phase 8) -- reuses the same counter and the same
//                  "log one violation event, tagged by rule" reporting shape
//                  Shared.Telemetry.Abstractions.Governance.
//                  TelemetryGovernanceEvaluator already established, rather
//                  than introducing a second, parallel governance-reporting
//                  mechanism.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Evaluates a CallerContext against AuthorizationGovernanceRules and reports any violation found.</summary>
public static class AuthorizationGovernanceEvaluator
{
    /// <summary>
    /// Validates <paramref name="caller"/>. If any violation is found, writes one
    /// AUTHORIZATION_GOVERNANCE_VIOLATION log event and increments
    /// PlantSafetyMetrics.GovernanceViolationsDetected once per violation. Returns the
    /// violation list (empty if none).
    /// </summary>
    public static IReadOnlyList<string> EvaluateAndReport(CallerContext? caller, string module, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        IReadOnlyList<string> violations = AuthorizationGovernanceRules.ValidateCallerContext(caller);
        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string _ in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", "ValidateCallerContext"));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "AUTHORIZATION_GOVERNANCE_VIOLATION",
            Category: LogCategory.Authorization,
            Severity: LogSeverity.Warning,
            Message: $"CallerContext for module \"{module}\" failed authorization governance validation ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?>
            {
                ["Module"] = module,
                ["Violations"] = violations,
            },
            CorrelationId: caller?.CorrelationId ?? "unknown",
            Module: "Authorization",
            OccurredAtUtc: clock.UtcNow));

        return violations;
    }
}
