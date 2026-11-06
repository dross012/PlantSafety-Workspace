// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           TenantGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Runs TenantGovernanceRules' pure
//                  validation and reports any violation found through the
//                  existing ILogEventWriter/PlantSafetyMetrics.
//                  GovernanceViolationsDetected counter -- the same counter
//                  and reporting shape AuthenticationGovernanceEvaluator
//                  (Phase 10), AuthorizationGovernanceEvaluator (Phase 9),
//                  and TelemetryGovernanceEvaluator (Phase 8) already
//                  established, not a fourth, parallel governance mechanism.
// ============================================================================
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Evaluates tenant resolution and isolation-decision shapes against TenantGovernanceRules and reports any violation found.</summary>
public static class TenantGovernanceEvaluator
{
    /// <summary>Validates <paramref name="resolution"/> and reports any violation via a TENANT_GOVERNANCE_VIOLATION log event.</summary>
    public static IReadOnlyList<string> EvaluateResolutionAndReport(TenantContextResolution? resolution, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        IReadOnlyList<string> violations = TenantGovernanceRules.ValidateResolution(resolution);
        Report(violations, resolution?.Context?.CorrelationId, "TenantContextResolution", clock, logEventWriter);
        return violations;
    }

    /// <summary>
    /// Validates a bare TenantContext (used by domain-layer wrappers that only receive the
    /// context, not the resolution it came from) and reports any violation via a
    /// TENANT_GOVERNANCE_VIOLATION log event.
    /// </summary>
    public static IReadOnlyList<string> EvaluateContextAndReport(TenantContext? tenantContext, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        IReadOnlyList<string> violations = TenantGovernanceRules.ValidateTenantContext(tenantContext);
        Report(violations, tenantContext?.CorrelationId, "TenantContext", clock, logEventWriter);
        return violations;
    }

    /// <summary>Validates a tenant-isolation decision and reports any violation via a TENANT_GOVERNANCE_VIOLATION log event.</summary>
    public static IReadOnlyList<string> EvaluateIsolationDecisionAndReport(
        TenantAccessDecision? decision, Guid? requestedFacilityId, string correlationId, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        IReadOnlyList<string> violations = TenantGovernanceRules.ValidateIsolationDecisionShape(decision, requestedFacilityId);
        Report(violations, correlationId, "TenantAccessDecision", clock, logEventWriter);
        return violations;
    }

    private static void Report(IReadOnlyList<string> violations, string? correlationId, string subject, IClock clock, ILogEventWriter logEventWriter)
    {
        if (violations.Count == 0)
        {
            return;
        }

        foreach (string violation in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", violation));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "TENANT_GOVERNANCE_VIOLATION",
            Category: LogCategory.Tenant,
            Severity: LogSeverity.Warning,
            Message: $"{subject} failed tenant governance validation ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?> { ["Subject"] = subject, ["Violations"] = violations },
            CorrelationId: correlationId ?? "unknown",
            Module: "Tenant",
            OccurredAtUtc: clock.UtcNow));
    }
}
