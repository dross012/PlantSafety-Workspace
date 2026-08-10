// ============================================================================
//  Project:        Deployment.Tools.CanaryRolloutDrill
//  File:           Program.cs
//  Created:        2026-08-10
//  Description:    Operationalization deliverable, same shape as
//                  SnapshotDrill/GovernanceDrill: a local/CI "staging
//                  simulation" of a canary rollout, run entirely against the
//                  real, unchanged Phase 18 CanaryManager/DeploymentEngine/
//                  ConfigRegistry/DriftReconciler. Five deterministic steps:
//                  (1) a fully-promoted canary (every stage gate passes),
//                  (2) DeploymentEngine finalizing that promotion, (3)
//                  publishing the resulting snapshot and confirming zero
//                  drift against the registry, (4) a transient gate flake at
//                  the first stage that recovers on retry without rollback,
//                  and (5) a simulated error-rate threshold breach at the
//                  50% stage that stops promotion and runs every
//                  previously-succeeded stage's compensation in reverse
//                  order -- the actual rollback trigger this phase's
//                  acceptance criteria describe. Emits a deterministic JSON
//                  evidence report and exits non-zero on the first failed
//                  assertion so it gates CI.
// ============================================================================
using System.Text.Json;

using Deployment.ConfigSchema.Contracts;
using Deployment.ConfigSchema.Registry;
using Deployment.Orchestration;
using Deployment.Orchestration.RolloutStrategies;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

const string TenantId = "acme-plant-1";
string outputPath = args.Length > 0 ? args[0] : "canary-rollout-drill-evidence.json";

var clock = new FixedClock(new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc));
var logWriter = new CapturingLogEventWriter();
var steps = new List<DrillStepResult>();
bool overallPassed = true;

DeploymentContractMetadata Metadata(string correlationId) =>
    new(DeploymentContractMetadata.CurrentVersion, TenantId, correlationId, clock.UtcNow);

TenantConfig BuildTenant() => new(
    Metadata("canary-drill-tenant"), TenantId, "Acme Plant 1", TenantTopology.Shared, TenantStatus.Active,
    new TenantResourceQuota(MaxMachineGroups: 25, MaxConcurrentDeployments: 3, MaxStorageBytes: 53_687_091_200),
    new[] { "facility" }, Array.Empty<string>());

ModuleConfig BuildModule() =>
    new(Metadata("canary-drill-module"), "facility", "Facility", ModuleLifecycleState.Enabled, Array.Empty<FeatureFlagConfig>(), Array.Empty<string>());

DeploymentConfig BuildDeploymentConfig() => new(
    Metadata("canary-drill-deployment"), "deploy-acme-plant-1-canary", "production", RolloutStrategyType.Canary,
    new[] { "facility" }, new[] { 10, 25, 50, 100 }, 60, 1);

async Task Step(string name, Func<Task<string>> action)
{
    try
    {
        string details = await action();
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

// Simulated per-stage error-rate metrics feeding the canary gate -- deterministic, fixed inputs,
// never randomized, mirroring how a real gate would read a monitoring backend's SLI.
var errorRateByStage = new Dictionary<int, double> { [10] = 0.01, [25] = 0.02, [50] = 0.08, [100] = 0.01 };
const double ErrorRateThreshold = 0.05;

Task<bool> AlwaysPassingGate(int trafficPercentage, CancellationToken _) => Task.FromResult(true);

// -------------------------------------------------------------------------------------------
// Step 1: every stage gate passes -- CanaryManager fully promotes to 100%.
// -------------------------------------------------------------------------------------------
CanaryResult? fullPromotion = null;
await Step("Canary stages [10,25,50,100] all pass -- fully promoted", async () =>
{
    fullPromotion = await CanaryManager.ExecuteAsync(new[] { 10, 25, 50, 100 }, AlwaysPassingGate);

    if (!fullPromotion.FullyPromoted || fullPromotion.ReachedTrafficPercentage != 100 || fullPromotion.StageResults.Count != 4)
    {
        throw new InvalidOperationException(
            $"Expected FullyPromoted=true, ReachedTrafficPercentage=100, 4 stage results; got FullyPromoted={fullPromotion.FullyPromoted}, ReachedTrafficPercentage={fullPromotion.ReachedTrafficPercentage}, StageResults={fullPromotion.StageResults.Count}.");
    }

    return $"ReachedTrafficPercentage=100, {fullPromotion.StageResults.Count} stage(s) passed";
});

// -------------------------------------------------------------------------------------------
// Step 2: DeploymentEngine finalizes the fully-promoted canary (deterministic, ordered,
// no compensation needed since every step succeeds).
// -------------------------------------------------------------------------------------------
DeploymentExecutionResult? finalizeResult = null;
await Step("DeploymentEngine finalizes the promotion (activate-100-percent, cleanup-old-version)", async () =>
{
    var finalizeSteps = new[]
    {
        new DeploymentStep("activate-100-percent", 0, TimeSpan.FromSeconds(30), 0, _ => Task.FromResult(true), null),
        new DeploymentStep("cleanup-old-version", 1, TimeSpan.FromSeconds(30), 0, _ => Task.FromResult(true), null),
    };

    finalizeResult = await DeploymentEngine.ExecuteAsync("deploy-acme-plant-1-canary", finalizeSteps, clock, logWriter, "canary-drill-finalize");

    if (!finalizeResult.Succeeded || finalizeResult.CompensationRan || finalizeResult.StepResults.Count != 2)
    {
        throw new InvalidOperationException(
            $"Expected Succeeded=true, CompensationRan=false, 2 step results; got Succeeded={finalizeResult.Succeeded}, CompensationRan={finalizeResult.CompensationRan}, StepResults={finalizeResult.StepResults.Count}.");
    }

    return $"{finalizeResult.StepResults.Count} step(s) succeeded, no compensation needed";
});

// -------------------------------------------------------------------------------------------
// Step 3: publish the resulting snapshot and confirm the registry shows zero drift.
// -------------------------------------------------------------------------------------------
await Step("Publish resulting ConfigSnapshot and reconcile against registry -- zero drift", () =>
{
    var registry = new ConfigRegistry();
    ConfigSnapshot published = registry.Publish(ConfigSnapshotFactory.Create(
        TenantId, 1, BuildDeploymentConfig(), new[] { BuildTenant() }, Array.Empty<RoleConfig>(), new[] { BuildModule() }, clock.UtcNow));

    DriftReport report = DriftReconciler.ReconcileAgainstRegistry(registry, published, clock, logWriter, "canary-drill-drift-check");

    if (report.DriftDetected)
    {
        throw new InvalidOperationException($"Expected zero drift, found {report.Diff.Changes.Count} change(s).");
    }

    return Task.FromResult("DriftDetected=false, snapshot matches registry exactly");
});

// -------------------------------------------------------------------------------------------
// Step 4: a transient monitoring flake at the first stage -- fails attempt 1, recovers on
// attempt 2 within the step's retry budget, without triggering any rollback.
// -------------------------------------------------------------------------------------------
await Step("Transient gate flake at 10% stage -- retried once, recovers without rollback", async () =>
{
    int attemptCount = 0;
    var flakyStep = new DeploymentStep(
        "canary-stage-10-gate",
        StepIndex: 0,
        Timeout: TimeSpan.FromSeconds(30),
        MaxRetries: 1,
        Action: _ =>
        {
            attemptCount++;
            return Task.FromResult(attemptCount >= 2); // fails attempt 1 (transient monitoring timeout), succeeds attempt 2
        },
        Compensation: null);

    DeploymentExecutionResult result = await DeploymentEngine.ExecuteAsync("deploy-acme-plant-1-canary", new[] { flakyStep }, clock, logWriter, "canary-drill-retry");

    if (!result.Succeeded || result.CompensationRan || result.StepResults[0].AttemptsUsed != 2)
    {
        throw new InvalidOperationException(
            $"Expected Succeeded=true, CompensationRan=false, AttemptsUsed=2; got Succeeded={result.Succeeded}, CompensationRan={result.CompensationRan}, AttemptsUsed={result.StepResults[0].AttemptsUsed}.");
    }

    return $"Recovered on attempt {result.StepResults[0].AttemptsUsed} of {flakyStep.MaxRetries + 1} allowed";
});

// -------------------------------------------------------------------------------------------
// Step 5: the actual rollback trigger -- the 50% stage's simulated error rate (0.08) breaches
// the 0.05 threshold. DeploymentEngine must stop advancing and compensate every
// previously-succeeded stage, in reverse order.
// -------------------------------------------------------------------------------------------
await Step("Error-rate threshold breached at 50% -- DeploymentEngine stops and compensates prior stages in reverse order", async () =>
{
    var compensationOrder = new List<string>();

    DeploymentStep StageStep(int trafficPercentage, int stepIndex) => new(
        $"canary-stage-{trafficPercentage}-gate",
        stepIndex,
        TimeSpan.FromSeconds(30),
        MaxRetries: 0,
        Action: _ => Task.FromResult(errorRateByStage[trafficPercentage] <= ErrorRateThreshold),
        Compensation: _ =>
        {
            compensationOrder.Add($"revert-{trafficPercentage}");
            return Task.CompletedTask;
        });

    var canarySteps = new[] { StageStep(10, 0), StageStep(25, 1), StageStep(50, 2), StageStep(100, 3) };
    DeploymentExecutionResult result = await DeploymentEngine.ExecuteAsync("deploy-acme-plant-1-canary", canarySteps, clock, logWriter, "canary-drill-rollback");

    var expectedCompensationOrder = new[] { "revert-25", "revert-10" };

    if (result.Succeeded
        || !result.CompensationRan
        || result.StepResults.Count != 3 // 10 (pass), 25 (pass), 50 (fail) -- 100 never attempted
        || !result.StepResults[2].FailureReason!.Contains("returned false", StringComparison.Ordinal)
        || !compensationOrder.SequenceEqual(expectedCompensationOrder))
    {
        throw new InvalidOperationException(
            $"Expected Succeeded=false, CompensationRan=true, 3 step results, compensation order [{string.Join(",", expectedCompensationOrder)}]; " +
            $"got Succeeded={result.Succeeded}, CompensationRan={result.CompensationRan}, StepResults={result.StepResults.Count}, CompensationOrder=[{string.Join(",", compensationOrder)}].");
    }

    return $"Stopped at 50% (error rate {errorRateByStage[50]:P0} > {ErrorRateThreshold:P0} threshold), reverted in order [{string.Join(" -> ", compensationOrder)}]";
});

var evidence = new DrillEvidence(
    "DeploymentEngine canary rollout drill",
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
