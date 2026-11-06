// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           SchemaGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. Runs SchemaGovernanceRules.ValidateAll
//                  and reports any violation found through the existing
//                  ILogEventWriter/PlantSafetyMetrics.GovernanceViolationsDetected
//                  counter -- one SCHEMA_GOVERNANCE_VIOLATION log event per
//                  call, same mechanism Phases 8-11 already established for
//                  their own governance evaluators (this does not replace
//                  those evaluators; it is a convenience composition of them).
// ============================================================================
using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Evaluates the composed identity/contract/tenant governance rules and reports any violation found.</summary>
public static class SchemaGovernanceEvaluator
{
    /// <summary>Validates all three resolutions/metadata together and reports any violation via a SCHEMA_GOVERNANCE_VIOLATION log event.</summary>
    public static IReadOnlyList<string> EvaluateAndReport(
        CallerContextResolution? callerResolution,
        TenantContextResolution? tenantResolution,
        ContractMetadata? contractMetadata,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        IReadOnlyList<string> violations = SchemaGovernanceRules.ValidateAll(callerResolution, tenantResolution, contractMetadata);
        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string violation in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", violation));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "SCHEMA_GOVERNANCE_VIOLATION",
            Category: LogCategory.Infrastructure,
            Severity: LogSeverity.Warning,
            Message: $"Composed schema governance validation failed ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?> { ["Violations"] = violations },
            CorrelationId: contractMetadata?.CorrelationId ?? tenantResolution?.Context.CorrelationId ?? "unknown",
            Module: "Schema",
            OccurredAtUtc: clock.UtcNow));

        return violations;
    }
}
