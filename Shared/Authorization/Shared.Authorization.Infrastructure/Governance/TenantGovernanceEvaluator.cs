// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           TenantGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 11 deliverable. Pure validation of TenantContext,
//                  TenantContextResolution, and TenantAccessDecision shapes,
//                  reported through the existing ILogEventWriter/
//                  PlantSafetyMetrics primitives. Every downstream module's
//                  *Tenant.cs calls EvaluateContextAndReport before running
//                  TenantIsolationCheck, and EvaluateIsolationDecisionAndReport
//                  after; ApiKeyAuthenticationMiddleware calls
//                  EvaluateResolutionAndReport once per request.
// ============================================================================
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Evaluates tenant shapes against pure validation rules and reports any violation found.</summary>
public static class TenantGovernanceEvaluator
{
    /// <summary>Validates <paramref name="tenantContext"/>'s shape and reports any violation found.</summary>
    public static IReadOnlyList<string> EvaluateContextAndReport(TenantContext tenantContext, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(tenantContext);

        var violations = new List<string>();
        if (string.IsNullOrWhiteSpace(tenantContext.TenantId))
        {
            violations.Add("TenantContext.TenantId is missing or empty.");
        }

        if (string.IsNullOrWhiteSpace(tenantContext.CorrelationId))
        {
            violations.Add("TenantContext.CorrelationId is missing or empty -- correlation propagation is broken for this tenant context.");
        }

        return Report(violations, "ValidateTenantContext", tenantContext.CorrelationId, clock, logEventWriter);
    }

    /// <summary>Validates <paramref name="resolution"/>'s shape and reports any violation found.</summary>
    public static IReadOnlyList<string> EvaluateResolutionAndReport(TenantContextResolution resolution, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        return EvaluateContextAndReport(resolution.Context, clock, logEventWriter);
    }

    /// <summary>Validates <paramref name="decision"/>'s shape and reports any violation found.</summary>
    public static IReadOnlyList<string> EvaluateIsolationDecisionAndReport(
        TenantAccessDecision decision,
        Guid? requestedFacilityId,
        string correlationId,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(decision);

        var violations = new List<string>();
        if (string.IsNullOrWhiteSpace(decision.Reason))
        {
            violations.Add("TenantAccessDecision.Reason is missing or empty -- every isolation decision must carry a human-readable reason.");
        }

        return Report(violations, "ValidateTenantAccessDecision", correlationId, clock, logEventWriter);
    }

    private static IReadOnlyList<string> Report(IReadOnlyList<string> violations, string ruleName, string correlationId, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string _ in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", ruleName));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "TENANT_GOVERNANCE_VIOLATION",
            Category: LogCategory.Tenant,
            Severity: LogSeverity.Warning,
            Message: $"Tenant shape failed governance rule \"{ruleName}\" ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?> { ["Rule"] = ruleName, ["Violations"] = violations },
            CorrelationId: string.IsNullOrWhiteSpace(correlationId) ? "unknown" : correlationId,
            Module: "Governance",
            OccurredAtUtc: clock.UtcNow));

        return violations;
    }
}
