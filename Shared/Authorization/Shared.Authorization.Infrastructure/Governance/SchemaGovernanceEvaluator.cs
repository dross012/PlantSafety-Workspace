// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           SchemaGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. The single composed identity/
//                  contract/tenant governance pass ApiKeyAuthenticationMiddleware
//                  runs once per request, on top of (not instead of) the
//                  per-layer AuthenticationGovernanceEvaluator/
//                  TenantGovernanceEvaluator calls already made earlier in
//                  the same request. Validates cross-shape consistency that
//                  no single-shape validator can see: the CorrelationId and
//                  TenantId threaded through the resolved CallerContextResolution,
//                  TenantContextResolution, and composed ContractMetadata
//                  must all agree -- a mismatch would mean the unified
//                  contract was composed from a stale or substituted piece
//                  of the request's identity, exactly the class of bug
//                  unified-schema governance exists to catch.
// ============================================================================
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Validates cross-shape consistency across a request's composed identity, tenant, and contract metadata.</summary>
public static class SchemaGovernanceEvaluator
{
    /// <summary>
    /// Validates that <paramref name="resolution"/>, <paramref name="tenantResolution"/>, and
    /// <paramref name="contractMetadata"/> agree on CorrelationId and TenantId. If any violation is
    /// found, writes one SCHEMA_GOVERNANCE_VIOLATION log event and increments
    /// PlantSafetyMetrics.GovernanceViolationsDetected once per violation.
    /// </summary>
    public static IReadOnlyList<string> EvaluateAndReport(
        CallerContextResolution resolution,
        TenantContextResolution tenantResolution,
        ContractMetadata contractMetadata,
        IClock clock,
        ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(tenantResolution);
        ArgumentNullException.ThrowIfNull(contractMetadata);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        var violations = new List<string>();

        if (resolution.Caller.CorrelationId != tenantResolution.Context.CorrelationId)
        {
            violations.Add($"CorrelationId mismatch between resolved CallerContext (\"{resolution.Caller.CorrelationId}\") and TenantContext (\"{tenantResolution.Context.CorrelationId}\").");
        }

        if (tenantResolution.Context.CorrelationId != contractMetadata.CorrelationId)
        {
            violations.Add($"CorrelationId mismatch between resolved TenantContext (\"{tenantResolution.Context.CorrelationId}\") and composed ContractMetadata (\"{contractMetadata.CorrelationId}\").");
        }

        if (tenantResolution.Context.TenantId != contractMetadata.TenantId)
        {
            violations.Add($"TenantId mismatch between resolved TenantContext (\"{tenantResolution.Context.TenantId}\") and composed ContractMetadata (\"{contractMetadata.TenantId}\").");
        }

        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string _ in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", "ValidateSchemaConsistency"));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "SCHEMA_GOVERNANCE_VIOLATION",
            Category: LogCategory.Authorization,
            Severity: LogSeverity.Warning,
            Message: $"Composed request identity/tenant/contract shapes failed schema governance consistency validation ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?> { ["Violations"] = violations },
            CorrelationId: string.IsNullOrWhiteSpace(contractMetadata.CorrelationId) ? "unknown" : contractMetadata.CorrelationId,
            Module: "Governance",
            OccurredAtUtc: clock.UtcNow));

        return violations;
    }
}
