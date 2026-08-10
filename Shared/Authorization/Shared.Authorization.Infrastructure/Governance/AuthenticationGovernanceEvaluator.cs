// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           AuthenticationGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. Pure validation of a
//                  CallerContextResolution's shape, reported through the
//                  existing ILogEventWriter/PlantSafetyMetrics primitives --
//                  mirrors TelemetryGovernanceEvaluator (Phase 8) exactly.
// ============================================================================
using Shared.Authorization.Abstractions.Identity;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Evaluates a CallerContextResolution against pure validation rules and reports any violation found.</summary>
public static class AuthenticationGovernanceEvaluator
{
    /// <summary>
    /// Validates <paramref name="resolution"/>. If any violation is found, writes one
    /// AUTHENTICATION_GOVERNANCE_VIOLATION log event and increments
    /// PlantSafetyMetrics.GovernanceViolationsDetected once per violation. Returns the violation list
    /// (empty if none).
    /// </summary>
    public static IReadOnlyList<string> EvaluateAndReport(CallerContextResolution resolution, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        var violations = new List<string>();

        if (string.IsNullOrWhiteSpace(resolution.Caller.CorrelationId))
        {
            violations.Add("CallerContext.CorrelationId is missing or empty -- correlation propagation is broken for this resolution.");
        }

        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string _ in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", "ValidateCallerContextResolution"));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "AUTHENTICATION_GOVERNANCE_VIOLATION",
            Category: LogCategory.Authentication,
            Severity: LogSeverity.Warning,
            Message: $"CallerContextResolution failed authentication governance validation ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?> { ["Violations"] = violations },
            CorrelationId: string.IsNullOrWhiteSpace(resolution.Caller.CorrelationId) ? "unknown" : resolution.Caller.CorrelationId,
            Module: "Governance",
            OccurredAtUtc: clock.UtcNow));

        return violations;
    }
}
