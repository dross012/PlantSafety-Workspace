// ============================================================================
//  Project:        Deployment
//  Module:         Governance
//  File:           DeploymentGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. The single enforcement point that
//                  BLOCKS a non-compliant deployment: runs ConfigValidator.
//                  ValidateSnapshot (structural validity) plus every
//                  DeploymentGovernanceRules policy check, and returns a
//                  DeploymentGovernanceDecision whose Allowed field
//                  DeploymentOrchestrator (Orchestration/) must check before
//                  running a single DeploymentEngine step. Any violation --
//                  structural or policy -- blocks the deployment; there is
//                  no partial-allow.
// ============================================================================
using Deployment.ConfigSchema.Contracts;
using Deployment.ConfigSchema.Registry;
using Deployment.Observability;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

namespace Deployment.Governance;

/// <summary>The deterministic decision DeploymentGovernanceEvaluator produces: whether a deployment is allowed to proceed, and why not if it isn't.</summary>
public sealed record DeploymentGovernanceDecision(bool Allowed, IReadOnlyList<string> Violations);

/// <summary>The single enforcement point that blocks a non-compliant deployment.</summary>
public static class DeploymentGovernanceEvaluator
{
    /// <summary>
    /// Evaluates <paramref name="snapshot"/> and <paramref name="tenant"/> against every structural and
    /// policy rule. If any violation is found, writes one DEPLOYMENT_GOVERNANCE_VIOLATION log event per
    /// rule category, one DEPLOYMENT_BLOCKED log event, and increments
    /// DeploymentMetrics.GovernanceViolationsDetected once per violation. Returns a
    /// DeploymentGovernanceDecision whose Allowed field the caller must check before proceeding.
    /// </summary>
    public static DeploymentGovernanceDecision Evaluate(
        ConfigSnapshot snapshot,
        TenantConfig tenant,
        int requestedConcurrentDeployments,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        var violations = new List<string>();
        violations.AddRange(ConfigValidator.ValidateSnapshot(snapshot));
        violations.AddRange(DeploymentGovernanceRules.ValidateTenantEligibleForDeployment(tenant));
        violations.AddRange(DeploymentGovernanceRules.ValidateResourceQuota(tenant, requestedConcurrentDeployments));

        Dictionary<string, RoleConfig> roleCatalog = snapshot.RoleConfigs.ToDictionary(r => r.RoleId, StringComparer.Ordinal);
        violations.AddRange(DeploymentGovernanceRules.ValidateProductionRequiresApprovalRole(snapshot.DeploymentConfig, roleCatalog, tenant.AssignedRoleIds));

        bool allowed = violations.Count == 0;

        if (!allowed)
        {
            foreach (string _ in violations)
            {
                DeploymentMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", "DeploymentGovernanceEvaluate"));
            }

            logEventWriter.Write(new LogEvent(
                EventId: GovernanceEvents.GovernanceViolation,
                Category: LogCategory.Deployment,
                Severity: LogSeverity.Warning,
                Message: $"Deployment \"{snapshot.DeploymentConfig.DeploymentId}\" for tenant \"{tenant.TenantId}\" failed governance evaluation ({violations.Count} violation(s)).",
                Payload: new Dictionary<string, object?>
                {
                    ["DeploymentId"] = snapshot.DeploymentConfig.DeploymentId,
                    ["TenantId"] = tenant.TenantId,
                    ["Violations"] = violations,
                },
                CorrelationId: correlationId,
                Module: "Governance",
                OccurredAtUtc: clock.UtcNow));

            logEventWriter.Write(new LogEvent(
                EventId: GovernanceEvents.DeploymentBlocked,
                Category: LogCategory.Deployment,
                Severity: LogSeverity.Error,
                Message: $"Deployment \"{snapshot.DeploymentConfig.DeploymentId}\" for tenant \"{tenant.TenantId}\" was BLOCKED by governance.",
                Payload: new Dictionary<string, object?>
                {
                    ["DeploymentId"] = snapshot.DeploymentConfig.DeploymentId,
                    ["TenantId"] = tenant.TenantId,
                    ["ViolationCount"] = violations.Count,
                },
                CorrelationId: correlationId,
                Module: "Governance",
                OccurredAtUtc: clock.UtcNow));
        }

        return new DeploymentGovernanceDecision(allowed, violations);
    }
}
