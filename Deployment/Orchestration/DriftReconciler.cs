// ============================================================================
//  Project:        Deployment
//  Module:         Orchestration
//  File:           DriftReconciler.cs
//  Created:        2026-08-10
//  Description:    Phase 18 deliverable. Deterministic drift detection:
//                  compares an actually-observed ConfigSnapshot against the
//                  ConfigRegistry's latest published snapshot for the same
//                  tenant, via the existing ConfigSchema.Registry.ConfigDiff
//                  (this phase, unchanged) -- never a second, parallel diff
//                  implementation. A non-empty diff is drift; the reported
//                  ConfigChanges ARE the reconciliation action list (each
//                  change names the exact field that must be corrected to
//                  return to the expected, published state) -- no separate
//                  "auto-fix" logic is invented, since applying an
//                  unreviewed automatic correction to a live deployment
//                  would itself be an invented side effect this phase's
//                  constraints prohibit.
// ============================================================================
using Deployment.ConfigSchema.Registry;
using Deployment.Observability;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

namespace Deployment.Orchestration;

/// <summary>The deterministic result of a single drift reconciliation pass.</summary>
public sealed record DriftReport(
    string TenantId,
    bool DriftDetected,
    ConfigDiffResult Diff,
    DateTime ReconciledAtUtc);

/// <summary>Deterministic drift detection between an observed snapshot and the registry's latest published snapshot.</summary>
public static class DriftReconciler
{
    /// <summary>
    /// Compares <paramref name="actualSnapshot"/> against <paramref name="expectedSnapshot"/> (typically
    /// the registry's latest published snapshot for the same tenant) via ConfigDiff.Compare, records
    /// telemetry, and returns a DriftReport. Same inputs always produce the same output.
    /// </summary>
    public static DriftReport Reconcile(
        ConfigSnapshot actualSnapshot,
        ConfigSnapshot expectedSnapshot,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(actualSnapshot);
        ArgumentNullException.ThrowIfNull(expectedSnapshot);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(logEventWriter);

        ConfigDiffResult diff = ConfigDiff.Compare(expectedSnapshot, actualSnapshot);

        DeploymentTelemetry.RecordDriftReconciled(actualSnapshot.TenantId, diff.HasChanges, diff.Changes.Count, clock, logEventWriter, correlationId);

        return new DriftReport(actualSnapshot.TenantId, diff.HasChanges, diff, clock.UtcNow);
    }

    /// <summary>
    /// Convenience overload: looks up <paramref name="registry"/>'s latest published snapshot for
    /// <paramref name="actualSnapshot"/>.TenantId and reconciles against it. Throws
    /// InvalidOperationException if no snapshot has ever been published for that tenant.
    /// </summary>
    public static DriftReport ReconcileAgainstRegistry(
        ConfigRegistry registry,
        ConfigSnapshot actualSnapshot,
        IClock clock,
        ILogEventWriter logEventWriter,
        string correlationId)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(actualSnapshot);

        ConfigSnapshot? expectedSnapshot = registry.GetLatest(actualSnapshot.TenantId);
        if (expectedSnapshot is null)
        {
            throw new InvalidOperationException($"No published snapshot exists for tenant \"{actualSnapshot.TenantId}\" to reconcile against.");
        }

        return Reconcile(actualSnapshot, expectedSnapshot, clock, logEventWriter, correlationId);
    }
}
