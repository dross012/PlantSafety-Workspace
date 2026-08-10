// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           AuthorizationGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 9 deliverable. Pure validation of a CallerContext's
//                  shape, run before every module's AuthorizationCheck.
//                  Evaluate call -- mirrors TelemetryGovernanceEvaluator
//                  (Phase 8) exactly.
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Evaluates a CallerContext against pure validation rules and reports any violation found.</summary>
public static class AuthorizationGovernanceEvaluator
{
    /// <summary>
    /// Validates <paramref name="caller"/>'s shape for use in module <paramref name="module"/>. If any
    /// violation is found, writes one AUTHORIZATION_GOVERNANCE_VIOLATION log event and increments
    /// PlantSafetyMetrics.GovernanceViolationsDetected once per violation.
    /// </summary>
    public static IReadOnlyList<string> EvaluateAndReport(CallerContext caller, string module, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(caller);
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        var violations = new List<string>();
        if (string.IsNullOrWhiteSpace(caller.CorrelationId))
        {
            violations.Add("CallerContext.CorrelationId is missing or empty -- correlation propagation is broken for this authorization check.");
        }

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
            Message: $"CallerContext failed authorization governance validation for module \"{module}\" ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?> { ["Module"] = module, ["Violations"] = violations },
            CorrelationId: string.IsNullOrWhiteSpace(caller.CorrelationId) ? "unknown" : caller.CorrelationId,
            Module: "Governance",
            OccurredAtUtc: clock.UtcNow));

        return violations;
    }
}
