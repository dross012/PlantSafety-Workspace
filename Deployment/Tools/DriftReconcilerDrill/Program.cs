// ============================================================================
//  Project:        Deployment.Tools.DriftReconcilerDrill
//  File:           Program.cs
//  Created:        2026-08-10
//  Description:    Operationalization deliverable, same shape as
//                  SnapshotDrill/GovernanceDrill/CanaryRolloutDrill: a
//                  local/CI "staging simulation" of drift detection and
//                  remediation, run entirely against the real, unchanged
//                  Phase 18 ConfigRegistry/ConfigSnapshotFactory/ConfigDiff/
//                  DriftReconciler. Five deterministic steps covering both
//                  directions drift happens in production: (1) a healthy
//                  baseline with zero drift, (2) an out-of-band manual
//                  change to the running environment that DriftReconciler
//                  must detect against the registry's declared state, (3)
//                  the documented remediation workflow (per Deployment/
//                  Runbooks/IncidentResponse.md step 4: force the
//                  environment back to the registry's declared state --
//                  DriftReconciler itself only reports the diff, it never
//                  auto-applies a fix, so this step models applying that
//                  diff's corrective action and re-verifying), (4) the
//                  opposite direction -- the registry's declared state
//                  advances to a new version before the running environment
//                  has been redeployed to match it, and (5) the environment
//                  catching up via a real deployment. Emits a deterministic
//                  JSON evidence report and exits non-zero on the first
//                  failed assertion so it gates CI.
// ============================================================================
using System.Text.Json;

using Deployment.ConfigSchema.Contracts;
using Deployment.ConfigSchema.Registry;
using Deployment.Orchestration;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

const string TenantId = "acme-plant-1";
string outputPath = args.Length > 0 ? args[0] : "drift-reconciler-drill-evidence.json";

var clock = new FixedClock(new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc));
var logWriter = new CapturingLogEventWriter();
var registry = new ConfigRegistry();
var steps = new List<DrillStepResult>();
bool overallPassed = true;

DeploymentContractMetadata Metadata(string correlationId) =>
    new(DeploymentContractMetadata.CurrentVersion, TenantId, correlationId, clock.UtcNow);

TenantConfig BuildTenant() => new(
    Metadata("drift-drill-tenant"), TenantId, "Acme Plant 1", TenantTopology.Shared, TenantStatus.Active,
    new TenantResourceQuota(MaxMachineGroups: 25, MaxConcurrentDeployments: 3, MaxStorageBytes: 53_687_091_200),
    new[] { "facility" }, Array.Empty<string>());

ModuleConfig BuildModule(ModuleLifecycleState state) =>
    new(Metadata("drift-drill-module"), "facility", "Facility", state, Array.Empty<FeatureFlagConfig>(), Array.Empty<string>());

DeploymentConfig BuildDeploymentConfig(int healthCheckTimeoutSeconds) => new(
    Metadata("drift-drill-deployment"), "deploy-acme-plant-1-drift", "production", RolloutStrategyType.BlueGreen,
    new[] { "facility" }, Array.Empty<int>(), healthCheckTimeoutSeconds, 3);

ConfigSnapshot BuildSnapshot(int version, int healthCheckTimeoutSeconds, ModuleLifecycleState moduleState) =>
    ConfigSnapshotFactory.Create(TenantId, version, BuildDeploymentConfig(healthCheckTimeoutSeconds), new[] { BuildTenant() }, Array.Empty<RoleConfig>(), new[] { BuildModule(moduleState) }, clock.UtcNow);

void Step(string name, Func<string> action)
{
    try
    {
        string details = action();
        steps.Add(new DrillStepResult(name, true, details));
        Console.WriteLine($"[PASS] {name} -- {details}");
    }
    catch (Exception ex)
    {
        overallPassed = false;
        steps.Add(new DrillStepResult(name, false, ex.Message));
        Console.WriteLine($"[FAIL] {name} -- {ex.Message}");
    }
}

// -------------------------------------------------------------------------------------------
// Step 1: publish the declared baseline (v1) and reconcile the actual, healthy environment
// against it -- zero drift.
// -------------------------------------------------------------------------------------------
Step("Publish baseline snapshot (v1) and reconcile a matching environment -- zero drift", () =>
{
    ConfigSnapshot v1 = registry.Publish(BuildSnapshot(1, healthCheckTimeoutSeconds: 60, ModuleLifecycleState.Enabled));
    ConfigSnapshot actual = BuildSnapshot(1, healthCheckTimeoutSeconds: 60, ModuleLifecycleState.Enabled);

    DriftReport report = DriftReconciler.ReconcileAgainstRegistry(registry, actual, clock, logWriter, "drift-drill-baseline");

    if (report.DriftDetected)
    {
        throw new InvalidOperationException($"Expected zero drift for a matching environment, found {report.Diff.Changes.Count} change(s).");
    }

    return $"Registry v{v1.SnapshotVersion} == observed environment, DriftDetected=false";
});

// -------------------------------------------------------------------------------------------
// Step 2: an out-of-band manual change disables the module directly on the running
// environment (bypassing ConfigRegistry entirely) -- DriftReconciler must detect it.
// -------------------------------------------------------------------------------------------
Step("Out-of-band manual change (module disabled outside ConfigRegistry) -- drift detected", () =>
{
    ConfigSnapshot driftedActual = BuildSnapshot(1, healthCheckTimeoutSeconds: 60, ModuleLifecycleState.Disabled);
    DriftReport report = DriftReconciler.ReconcileAgainstRegistry(registry, driftedActual, clock, logWriter, "drift-drill-detect-manual");

    var expectedChange = new ConfigChange("ModuleConfig[facility].State", "Enabled", "Disabled");
    if (!report.DriftDetected || report.Diff.Changes.Count != 1 || report.Diff.Changes[0] != expectedChange)
    {
        string actual = string.Join(", ", report.Diff.Changes.Select(c => $"{c.FieldPath}: '{c.OldValue}' -> '{c.NewValue}'"));
        throw new InvalidOperationException($"Expected exactly one change ({expectedChange.FieldPath}: '{expectedChange.OldValue}' -> '{expectedChange.NewValue}'), found [{actual}].");
    }

    return $"DriftDetected=true, {report.Diff.Changes[0].FieldPath}: '{report.Diff.Changes[0].OldValue}' -> '{report.Diff.Changes[0].NewValue}'";
});

// -------------------------------------------------------------------------------------------
// Step 3: remediation -- per IncidentResponse.md/RollbackProcedure.md, DriftReconciler only
// reports the diff (it never auto-applies a fix); this step models an operator applying that
// diff's corrective action (forcing the environment back to the registry's declared state) and
// re-verifying via a second reconciliation pass.
// -------------------------------------------------------------------------------------------
Step("Remediation applied (environment forced back to declared state) -- drift resolved", () =>
{
    ConfigSnapshot remediatedActual = BuildSnapshot(1, healthCheckTimeoutSeconds: 60, ModuleLifecycleState.Enabled);
    DriftReport report = DriftReconciler.ReconcileAgainstRegistry(registry, remediatedActual, clock, logWriter, "drift-drill-remediate");

    if (report.DriftDetected)
    {
        throw new InvalidOperationException($"Expected zero drift after remediation, found {report.Diff.Changes.Count} change(s).");
    }

    return "Environment realigned with registry v1, DriftDetected=false";
});

// -------------------------------------------------------------------------------------------
// Step 4: the opposite direction -- the declared state advances (a new snapshot is published
// with a longer health check timeout) before the running environment has been redeployed to
// match it. DriftReconciler must detect the environment as stale relative to the new baseline.
// -------------------------------------------------------------------------------------------
Step("Declared state advances (v2 published) while environment still runs v1 -- drift detected", () =>
{
    ConfigSnapshot v2 = registry.Publish(BuildSnapshot(2, healthCheckTimeoutSeconds: 90, ModuleLifecycleState.Enabled));
    ConfigSnapshot staleActual = BuildSnapshot(1, healthCheckTimeoutSeconds: 60, ModuleLifecycleState.Enabled);

    DriftReport report = DriftReconciler.ReconcileAgainstRegistry(registry, staleActual, clock, logWriter, "drift-drill-detect-stale");

    var expectedChange = new ConfigChange("DeploymentConfig.HealthCheckTimeoutSeconds", "90", "60");
    if (!report.DriftDetected || report.Diff.Changes.Count != 1 || report.Diff.Changes[0] != expectedChange)
    {
        string actual = string.Join(", ", report.Diff.Changes.Select(c => $"{c.FieldPath}: '{c.OldValue}' -> '{c.NewValue}'"));
        throw new InvalidOperationException($"Expected exactly one change ({expectedChange.FieldPath}: '{expectedChange.OldValue}' -> '{expectedChange.NewValue}'), found [{actual}].");
    }

    return $"Registry v{v2.SnapshotVersion} declared, environment still v1 -- DriftDetected=true, {report.Diff.Changes[0].FieldPath}: '{report.Diff.Changes[0].OldValue}' -> '{report.Diff.Changes[0].NewValue}'";
});

// -------------------------------------------------------------------------------------------
// Step 5: a real deployment brings the environment up to date with the new declared state --
// zero drift again.
// -------------------------------------------------------------------------------------------
Step("Environment redeployed to v2 -- drift resolved", () =>
{
    ConfigSnapshot updatedActual = BuildSnapshot(2, healthCheckTimeoutSeconds: 90, ModuleLifecycleState.Enabled);
    DriftReport report = DriftReconciler.ReconcileAgainstRegistry(registry, updatedActual, clock, logWriter, "drift-drill-catch-up");

    if (report.DriftDetected)
    {
        throw new InvalidOperationException($"Expected zero drift after redeployment, found {report.Diff.Changes.Count} change(s).");
    }

    return "Environment realigned with registry v2, DriftDetected=false";
});

var evidence = new DrillEvidence(
    "DriftReconciler detection and remediation drill",
    TenantId,
    clock.UtcNow,
    steps,
    overallPassed);

string json = JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(outputPath, json);

Console.WriteLine();
Console.WriteLine(overallPassed
    ? $"=== DRILL PASSED ({steps.Count} steps) -- evidence written to {outputPath} ==="
    : $"=== DRILL FAILED -- evidence written to {outputPath} ===");

return overallPassed ? 0 : 1;

sealed record DrillStepResult(string StepName, bool Passed, string Details);

sealed record DrillEvidence(
    string Drill,
    string TenantId,
    DateTime RunAtUtc,
    IReadOnlyList<DrillStepResult> Steps,
    bool Passed);

sealed class CapturingLogEventWriter : ILogEventWriter
{
    public List<ILogEvent> Events { get; } = new();

    public void Write(ILogEvent logEvent) => Events.Add(logEvent);
}
