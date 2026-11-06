// ============================================================================
//  Project:        Deployment
//  Module:         Observability
//  File:           DeploymentTelemetry.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Deterministic logging and metrics
//                  recording for config snapshot publication and deployment
//                  step execution, via Shared.Telemetry's existing
//                  ILogEventWriter/PlantSafetyMetrics -- mirrors every prior
//                  phase's *Telemetry.cs pattern. DeploymentEngine (Orchestration/)
//                  calls RecordStepExecuted once per step, after running the
//                  step itself through Shared.Telemetry.Abstractions.
//                  Reliability.ReliabilityHooks.ExecuteAsync for duration/
//                  failure recording -- this class adds the Deployment-specific
//                  metric/log on top, it does not duplicate ReliabilityHooks.
// ============================================================================
using Deployment.ConfigSchema.Registry;
using Deployment.Governance;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

namespace Deployment.Observability;

/// <summary>Deterministic logging and metrics recording for Deployment operations.</summary>
public static class DeploymentTelemetry
{
    private const string Module = "Deployment";

    /// <summary>Records that <paramref name="snapshot"/> was published: increments SnapshotsPublished and writes a structured log event.</summary>
    public static void RecordSnapshotPublished(ConfigSnapshot snapshot, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        DeploymentMetrics.SnapshotsPublished.Add(1, new KeyValuePair<string, object?>("tenant", snapshot.TenantId));

        logEventWriter.Write(new LogEvent(
            EventId: GovernanceEvents.SnapshotPublished,
            Category: LogCategory.Deployment,
            Severity: LogSeverity.Info,
            Message: $"ConfigSnapshot \"{snapshot.SnapshotId}\" published for tenant \"{snapshot.TenantId}\" (version {snapshot.SnapshotVersion}).",
            Payload: new Dictionary<string, object?>
            {
                ["TenantId"] = snapshot.TenantId,
                ["SnapshotVersion"] = snapshot.SnapshotVersion,
                ["ContentHash"] = snapshot.ContentHash,
            },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));
    }

    /// <summary>Records that a deployment step ran: increments StepsExecuted (tagged by outcome) and writes a structured log event.</summary>
    public static void RecordStepExecuted(string stepName, bool succeeded, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(stepName);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        DeploymentMetrics.StepsExecuted.Add(1, new KeyValuePair<string, object?>("outcome", succeeded ? "Succeeded" : "Failed"));

        logEventWriter.Write(new LogEvent(
            EventId: GovernanceEvents.StepExecuted,
            Category: LogCategory.Deployment,
            Severity: succeeded ? LogSeverity.Info : LogSeverity.Error,
            Message: $"Deployment step \"{stepName}\" {(succeeded ? "succeeded" : "failed")}.",
            Payload: new Dictionary<string, object?>
            {
                ["StepName"] = stepName,
                ["Succeeded"] = succeeded,
            },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));
    }

    /// <summary>Records a drift reconciliation pass: increments DriftReconciliationsPerformed (tagged by whether drift was found) and writes a structured log event.</summary>
    public static void RecordDriftReconciled(string tenantId, bool driftFound, int changeCount, IClock clock, ILogEventWriter logEventWriter, string correlationId)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        DeploymentMetrics.DriftReconciliationsPerformed.Add(1, new KeyValuePair<string, object?>("driftFound", driftFound));

        logEventWriter.Write(new LogEvent(
            EventId: GovernanceEvents.DriftReconciled,
            Category: LogCategory.Deployment,
            Severity: driftFound ? LogSeverity.Warning : LogSeverity.Info,
            Message: $"Drift reconciliation for tenant \"{tenantId}\": driftFound={driftFound}, {changeCount} change(s).",
            Payload: new Dictionary<string, object?>
            {
                ["TenantId"] = tenantId,
                ["DriftFound"] = driftFound,
                ["ChangeCount"] = changeCount,
            },
            CorrelationId: correlationId,
            Module: Module,
            OccurredAtUtc: clock.UtcNow));
    }
}
