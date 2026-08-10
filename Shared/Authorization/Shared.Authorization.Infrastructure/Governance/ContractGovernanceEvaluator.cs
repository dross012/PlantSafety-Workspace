// ============================================================================
//  Project:        Shared.Authorization.Infrastructure
//  Module:         Governance
//  File:           ContractGovernanceEvaluator.cs
//  Created:        2026-08-10
//  Description:    Phase 12 deliverable. Pure validation of a ContractMetadata's
//                  shape, run by every module's *Tenant.cs immediately after
//                  composing one via ContractMetadataFactory.Create.
// ============================================================================
using Shared.Authorization.Abstractions.Contracts;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Metrics;

namespace Shared.Authorization.Infrastructure.Governance;

/// <summary>Evaluates a ContractMetadata against pure validation rules and reports any violation found.</summary>
public static class ContractGovernanceEvaluator
{
    /// <summary>
    /// Validates <paramref name="metadata"/>'s shape. If any violation is found, writes one
    /// CONTRACT_GOVERNANCE_VIOLATION log event and increments
    /// PlantSafetyMetrics.GovernanceViolationsDetected once per violation.
    /// </summary>
    public static IReadOnlyList<string> EvaluateAndReport(ContractMetadata metadata, IClock clock, ILogEventWriter logEventWriter)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        var violations = new List<string>();

        if (metadata.SchemaVersion != ContractMetadata.CurrentSchemaVersion)
        {
            violations.Add($"SchemaVersion {metadata.SchemaVersion} does not match the current ContractMetadata schema version ({ContractMetadata.CurrentSchemaVersion}).");
        }

        if (string.IsNullOrWhiteSpace(metadata.TenantId))
        {
            violations.Add("TenantId is missing or empty -- tenant propagation is broken for this contract.");
        }

        if (string.IsNullOrWhiteSpace(metadata.CorrelationId))
        {
            violations.Add("CorrelationId is missing or empty -- correlation propagation is broken for this contract.");
        }

        if (metadata.IssuedAtUtc == default)
        {
            violations.Add("IssuedAtUtc is unset (default DateTime) -- every contract must carry a real, deterministic-clock-sourced timestamp.");
        }

        if (violations.Count == 0)
        {
            return violations;
        }

        foreach (string _ in violations)
        {
            PlantSafetyMetrics.GovernanceViolationsDetected.Add(1, new KeyValuePair<string, object?>("rule", "ValidateContractMetadata"));
        }

        logEventWriter.Write(new LogEvent(
            EventId: "CONTRACT_GOVERNANCE_VIOLATION",
            Category: LogCategory.Authorization,
            Severity: LogSeverity.Warning,
            Message: $"ContractMetadata for tenant \"{metadata.TenantId}\" failed contract governance validation ({violations.Count} violation(s)).",
            Payload: new Dictionary<string, object?> { ["TenantId"] = metadata.TenantId, ["Violations"] = violations },
            CorrelationId: string.IsNullOrWhiteSpace(metadata.CorrelationId) ? "unknown" : metadata.CorrelationId,
            Module: "Governance",
            OccurredAtUtc: clock.UtcNow));

        return violations;
    }
}
