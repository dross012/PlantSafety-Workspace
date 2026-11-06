// ============================================================================
//  Project:        Deployment.Tools.SnapshotDrill
//  File:           Program.cs
//  Created:        2026-08-10
//  Description:    Operationalization deliverable: a local/CI "staging
//                  simulation" of a ConfigRegistry snapshot/rollback drill,
//                  run entirely against the real, unchanged Phase 18
//                  ConfigRegistry/ConfigSnapshotFactory/ConfigValidator/
//                  ConfigDiff/AuditLog -- no test doubles for the code under
//                  drill, only a minimal ILogEventWriter sink since this
//                  tool has no host DI container. The drill: publish a known
//                  snapshot (v1), validate it, publish a changed snapshot
//                  (v2), diff v1 against v2 and assert the expected field
//                  changed, roll back to v1 (creating v3), assert v3's
//                  ContentHash exactly matches v1's (same content, new
//                  version -- the documented RollbackProcedure.md contract),
//                  validate v3, and verify the audit hash chain is intact.
//                  Emits a deterministic JSON evidence report (step-by-step
//                  pass/fail, snapshot ids, content hashes, diff, audit
//                  entries) to stdout and to an output file, and exits
//                  non-zero on the first failed assertion so it gates CI.
// ============================================================================
using System.Text.Json;

using Deployment.ConfigSchema.Contracts;
using Deployment.ConfigSchema.Registry;
using Deployment.Observability;

using Shared.Telemetry.Abstractions.Events;

const string TenantId = "acme-plant-1";
string outputPath = args.Length > 0 ? args[0] : "snapshot-drill-evidence.json";

var logWriter = new CapturingLogEventWriter();
var registry = new ConfigRegistry();
var auditLog = new AuditLog();
var steps = new List<DrillStepResult>();
DateTime drillStartedAtUtc = new(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc);

DeploymentContractMetadata Metadata(string correlationId) =>
    new(DeploymentContractMetadata.CurrentVersion, TenantId, correlationId, drillStartedAtUtc);

DeploymentConfig BuildDeploymentConfig(int healthCheckTimeoutSeconds) => new(
    Metadata("drill-deployment"), "deploy-acme-plant-1-drill", "staging", RolloutStrategyType.BlueGreen,
    new[] { "facility", "scheduling" }, Array.Empty<int>(), healthCheckTimeoutSeconds, 3);

ModuleConfig[] moduleConfigs =
[
    new(Metadata("drill-module-facility"), "facility", "Facility", ModuleLifecycleState.Enabled, Array.Empty<FeatureFlagConfig>(), Array.Empty<string>()),
    new(Metadata("drill-module-scheduling"), "scheduling", "Scheduling", ModuleLifecycleState.Enabled, Array.Empty<FeatureFlagConfig>(), new[] { "facility" }),
];

RoleConfig[] roleConfigs =
[
    new(Metadata("drill-role-viewer"), "deployment-viewer", "Deployment Viewer", Array.Empty<string>(), new[] { DeploymentPermission.ViewDeployments }, false),
    new(Metadata("drill-role-operator"), "deployment-operator", "Deployment Operator", new[] { "deployment-viewer" }, new[] { DeploymentPermission.ExecuteDeployments }, false),
    new(Metadata("drill-role-admin"), "deployment-admin", "Deployment Admin", new[] { "deployment-operator" },
        new[] { DeploymentPermission.ManageConfig, DeploymentPermission.ManageTenants }, true),
];

TenantConfig BuildTenant() => new(
    Metadata("drill-tenant"), TenantId, "Acme Plant 1", TenantTopology.Shared, TenantStatus.Active,
    new TenantResourceQuota(MaxMachineGroups: 25, MaxConcurrentDeployments: 3, MaxStorageBytes: 53_687_091_200),
    new[] { "facility", "scheduling" }, new[] { "deployment-viewer", "deployment-operator", "deployment-admin" });

bool overallPassed = true;

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

ConfigSnapshot? v1 = null;
ConfigSnapshot? v2 = null;
ConfigSnapshot? v3 = null;

Step("Build and validate initial snapshot (v1)", () =>
{
    v1 = ConfigSnapshotFactory.Create(TenantId, 1, BuildDeploymentConfig(60), new[] { BuildTenant() }, roleConfigs, moduleConfigs, drillStartedAtUtc);
    IReadOnlyList<string> violations = ConfigValidator.ValidateSnapshot(v1);
    if (violations.Count > 0)
    {
        throw new InvalidOperationException($"v1 failed validation: {string.Join("; ", violations)}");
    }

    return $"SnapshotId={v1.SnapshotId}, ContentHash={v1.ContentHash}";
});

Step("Publish v1 to ConfigRegistry", () =>
{
    registry.Publish(v1!);
    auditLog.Append(TenantId, "ConfigPublished", "drill-operator", "drill-publish-v1", drillStartedAtUtc, logWriter);
    return $"Published SnapshotVersion={registry.GetLatest(TenantId)!.SnapshotVersion}";
});

Step("Build, validate, and publish changed snapshot (v2: HealthCheckTimeoutSeconds 60 -> 90)", () =>
{
    v2 = ConfigSnapshotFactory.Create(TenantId, 2, BuildDeploymentConfig(90), new[] { BuildTenant() }, roleConfigs, moduleConfigs, drillStartedAtUtc.AddMinutes(5));
    IReadOnlyList<string> violations = ConfigValidator.ValidateSnapshot(v2);
    if (violations.Count > 0)
    {
        throw new InvalidOperationException($"v2 failed validation: {string.Join("; ", violations)}");
    }

    registry.Publish(v2);
    auditLog.Append(TenantId, "ConfigPublished", "drill-operator", "drill-publish-v2", drillStartedAtUtc.AddMinutes(5), logWriter);
    return $"SnapshotId={v2.SnapshotId}, ContentHash={v2.ContentHash}";
});

ConfigDiffResult? diff = null;
Step("Diff v1 -> v2 and assert exactly the expected field changed", () =>
{
    diff = ConfigDiff.Compare(v1!, v2!);
    var expected = new ConfigChange("DeploymentConfig.HealthCheckTimeoutSeconds", "60", "90");

    if (!diff.HasChanges || diff.Changes.Count != 1 || diff.Changes[0] != expected)
    {
        string actual = string.Join(", ", diff.Changes.Select(c => $"{c.FieldPath}: '{c.OldValue}' -> '{c.NewValue}'"));
        throw new InvalidOperationException($"Expected exactly one change ({expected.FieldPath}: '{expected.OldValue}' -> '{expected.NewValue}'), found [{actual}].");
    }

    return $"{diff.Changes[0].FieldPath}: '{diff.Changes[0].OldValue}' -> '{diff.Changes[0].NewValue}'";
});

Step("Roll back to v1 (creates v3) via ConfigRegistry.RollbackTo", () =>
{
    v3 = registry.RollbackTo(TenantId, 1, drillStartedAtUtc.AddMinutes(10));
    auditLog.Append(TenantId, "ConfigRolledBack", "drill-operator", "drill-rollback-to-v1", drillStartedAtUtc.AddMinutes(10), logWriter);
    return $"SnapshotId={v3.SnapshotId}, SnapshotVersion={v3.SnapshotVersion}, ContentHash={v3.ContentHash}";
});

Step("Assert v3.ContentHash exactly matches v1.ContentHash (same content, new version)", () =>
{
    if (!string.Equals(v3!.ContentHash, v1!.ContentHash, StringComparison.Ordinal))
    {
        throw new InvalidOperationException($"v3.ContentHash ({v3.ContentHash}) != v1.ContentHash ({v1.ContentHash})");
    }

    return "ContentHash match confirmed";
});

Step("Validate rolled-back snapshot (v3)", () =>
{
    IReadOnlyList<string> violations = ConfigValidator.ValidateSnapshot(v3!);
    if (violations.Count > 0)
    {
        throw new InvalidOperationException($"v3 failed validation: {string.Join("; ", violations)}");
    }

    return "v3 valid";
});

Step("Assert ConfigRegistry history for tenant has exactly 3 versions", () =>
{
    IReadOnlyList<ConfigSnapshot> history = registry.ListVersions(TenantId);
    if (history.Count != 3)
    {
        throw new InvalidOperationException($"Expected 3 versions, found {history.Count}.");
    }

    return $"Versions: [{string.Join(", ", history.Select(s => s.SnapshotVersion))}]";
});

Step("Verify AuditLog hash-chain integrity for tenant", () =>
{
    bool intact = auditLog.VerifyChainIntegrity(TenantId);
    if (!intact)
    {
        throw new InvalidOperationException("Audit hash chain is broken.");
    }

    IReadOnlyList<AuditEntry> entries = auditLog.GetEntries(TenantId);
    return $"Chain intact, {entries.Count} entries, {logWriter.Events.Count} telemetry events emitted";
});

DateTime drillCompletedAtUtc = drillStartedAtUtc.AddMinutes(10);

var evidence = new DrillEvidence(
    "ConfigRegistry snapshot/rollback drill",
    TenantId,
    drillStartedAtUtc,
    drillCompletedAtUtc,
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
    DateTime StartedAtUtc,
    DateTime CompletedAtUtc,
    IReadOnlyList<DrillStepResult> Steps,
    bool Passed);

sealed class CapturingLogEventWriter : ILogEventWriter
{
    public List<ILogEvent> Events { get; } = new();

    public void Write(ILogEvent logEvent) => Events.Add(logEvent);
}
