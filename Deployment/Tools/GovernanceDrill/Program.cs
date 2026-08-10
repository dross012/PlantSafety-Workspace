// ============================================================================
//  Project:        Deployment.Tools.GovernanceDrill
//  File:           Program.cs
//  Created:        2026-08-10
//  Description:    Operationalization deliverable, same shape as
//                  Deployment.Tools.SnapshotDrill: a local/CI "staging
//                  simulation" that exercises the real, unchanged Phase 18
//                  DeploymentGovernanceEvaluator/DeploymentGovernanceRules/
//                  ConfigValidator pipeline against five deterministic
//                  scenarios -- one compliant staging deployment, three
//                  independently non-compliant deployments (suspended
//                  tenant, quota exceeded, production without an
//                  approval-capable role), and one compliant production
//                  deployment (approval-capable role assigned) -- asserting
//                  DeploymentGovernanceEvaluator.Evaluate's Allowed/
//                  Violations outcome and the exact GOVERNANCE_VIOLATION/
//                  DEPLOYMENT_BLOCKED telemetry events emitted for each.
//                  Emits a deterministic JSON evidence report and exits
//                  non-zero on the first mismatch so it gates CI.
// ============================================================================
using System.Text.Json;

using Deployment.ConfigSchema.Contracts;
using Deployment.ConfigSchema.Registry;
using Deployment.Governance;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

const string TenantId = "regulated-plant-9";
string outputPath = args.Length > 0 ? args[0] : "governance-drill-evidence.json";

var clock = new FixedClock(new DateTime(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc));
var scenarios = new List<ScenarioResult>();
bool overallPassed = true;

DeploymentContractMetadata Metadata(string correlationId) =>
    new(DeploymentContractMetadata.CurrentVersion, TenantId, correlationId, clock.UtcNow);

ModuleConfig[] moduleConfigs =
[
    new(Metadata("gov-drill-module-facility"), "facility", "Facility", ModuleLifecycleState.Enabled, Array.Empty<FeatureFlagConfig>(), Array.Empty<string>()),
];

RoleConfig[] roleConfigs =
[
    new(Metadata("gov-drill-role-viewer"), "deployment-viewer", "Deployment Viewer", Array.Empty<string>(), new[] { DeploymentPermission.ViewDeployments }, false),
    new(Metadata("gov-drill-role-operator"), "deployment-operator", "Deployment Operator", new[] { "deployment-viewer" }, new[] { DeploymentPermission.ExecuteDeployments }, false),
    new(Metadata("gov-drill-role-approver"), "deployment-approver", "Deployment Approver", Array.Empty<string>(), new[] { DeploymentPermission.ApproveDeployments }, true),
];

DeploymentConfig BuildDeploymentConfig(string environmentName) => new(
    Metadata("gov-drill-deployment"), $"deploy-{TenantId}-{environmentName}", environmentName, RolloutStrategyType.BlueGreen,
    new[] { "facility" }, Array.Empty<int>(), 60, 3);

TenantConfig BuildTenant(TenantStatus status, IReadOnlyList<string> assignedRoleIds) => new(
    Metadata("gov-drill-tenant"), TenantId, "Regulated Plant 9", TenantTopology.Isolated, status,
    new TenantResourceQuota(MaxMachineGroups: 60, MaxConcurrentDeployments: 3, MaxStorageBytes: 214_748_364_800),
    new[] { "facility" }, assignedRoleIds);

ConfigSnapshot BuildSnapshot(int version, DeploymentConfig deploymentConfig, TenantConfig tenant) =>
    ConfigSnapshotFactory.Create(TenantId, version, deploymentConfig, new[] { tenant }, roleConfigs, moduleConfigs, clock.UtcNow);

void RunScenario(string name, ConfigSnapshot snapshot, TenantConfig tenant, int requestedConcurrentDeployments, bool expectedAllowed, string? expectedViolationSubstring)
{
    var logWriter = new CapturingLogEventWriter();

    IReadOnlyList<string> structuralViolations = ConfigValidator.ValidateSnapshot(snapshot);
    if (structuralViolations.Count > 0)
    {
        overallPassed = false;
        scenarios.Add(new ScenarioResult(name, false, expectedAllowed, false, structuralViolations,
            $"Snapshot failed structural validation before governance could even run: {string.Join("; ", structuralViolations)}", 0));
        Console.WriteLine($"[FAIL] {name} -- structural validation failed unexpectedly");
        return;
    }

    DeploymentGovernanceDecision decision = DeploymentGovernanceEvaluator.Evaluate(
        snapshot, tenant, requestedConcurrentDeployments, clock, logWriter, correlationId: $"gov-drill-{name}");

    bool allowedMatches = decision.Allowed == expectedAllowed;
    bool violationMatches = expectedAllowed
        ? decision.Violations.Count == 0
        : decision.Violations.Any(v => expectedViolationSubstring is not null && v.Contains(expectedViolationSubstring, StringComparison.Ordinal));

    int expectedEventCount = expectedAllowed ? 0 : 2; // one GOVERNANCE_VIOLATION + one DEPLOYMENT_BLOCKED
    bool eventCountMatches = logWriter.Events.Count == expectedEventCount;

    bool passed = allowedMatches && violationMatches && eventCountMatches;
    overallPassed &= passed;

    string details = passed
        ? $"Allowed={decision.Allowed} (expected {expectedAllowed}), {decision.Violations.Count} violation(s), {logWriter.Events.Count} telemetry event(s) emitted as expected"
        : $"MISMATCH: Allowed={decision.Allowed} (expected {expectedAllowed}), Violations=[{string.Join(" | ", decision.Violations)}], EventCount={logWriter.Events.Count} (expected {expectedEventCount})";

    scenarios.Add(new ScenarioResult(name, passed, expectedAllowed, decision.Allowed, decision.Violations, details, logWriter.Events.Count));
    Console.WriteLine($"[{(passed ? "PASS" : "FAIL")}] {name} -- {details}");
}

// Scenario A: compliant staging deployment -- Active tenant, quota within bounds, non-production
// environment (no approval-role requirement). Governance must allow it and emit zero events.
RunScenario(
    "Compliant staging deployment",
    BuildSnapshot(1, BuildDeploymentConfig("staging"), BuildTenant(TenantStatus.Active, new[] { "deployment-viewer", "deployment-operator" })),
    BuildTenant(TenantStatus.Active, new[] { "deployment-viewer", "deployment-operator" }),
    requestedConcurrentDeployments: 2,
    expectedAllowed: true,
    expectedViolationSubstring: null);

// Scenario B: suspended tenant -- must be blocked by ValidateTenantEligibleForDeployment.
RunScenario(
    "Suspended tenant blocked",
    BuildSnapshot(2, BuildDeploymentConfig("staging"), BuildTenant(TenantStatus.Suspended, new[] { "deployment-viewer", "deployment-operator" })),
    BuildTenant(TenantStatus.Suspended, new[] { "deployment-viewer", "deployment-operator" }),
    requestedConcurrentDeployments: 2,
    expectedAllowed: false,
    expectedViolationSubstring: "is not Active");

// Scenario C: quota exceeded -- requested concurrent deployments (5) exceeds the tenant's quota (3).
RunScenario(
    "Quota exceeded blocked",
    BuildSnapshot(3, BuildDeploymentConfig("staging"), BuildTenant(TenantStatus.Active, new[] { "deployment-viewer", "deployment-operator" })),
    BuildTenant(TenantStatus.Active, new[] { "deployment-viewer", "deployment-operator" }),
    requestedConcurrentDeployments: 5,
    expectedAllowed: false,
    expectedViolationSubstring: "exceeding its quota");

// Scenario D: production deployment with no approval-capable role assigned -- must be blocked.
RunScenario(
    "Production without approval role blocked",
    BuildSnapshot(4, BuildDeploymentConfig("production"), BuildTenant(TenantStatus.Active, new[] { "deployment-viewer", "deployment-operator" })),
    BuildTenant(TenantStatus.Active, new[] { "deployment-viewer", "deployment-operator" }),
    requestedConcurrentDeployments: 2,
    expectedAllowed: false,
    expectedViolationSubstring: "no assigned role grants ApproveDeployments");

// Scenario E: production deployment WITH an approval-capable role assigned -- must be allowed.
RunScenario(
    "Compliant production deployment (approver assigned)",
    BuildSnapshot(5, BuildDeploymentConfig("production"), BuildTenant(TenantStatus.Active, new[] { "deployment-viewer", "deployment-operator", "deployment-approver" })),
    BuildTenant(TenantStatus.Active, new[] { "deployment-viewer", "deployment-operator", "deployment-approver" }),
    requestedConcurrentDeployments: 2,
    expectedAllowed: true,
    expectedViolationSubstring: null);

var evidence = new DrillEvidence(
    "DeploymentGovernanceEvaluator blocking drill",
    TenantId,
    clock.UtcNow,
    scenarios,
    overallPassed);

string json = JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(outputPath, json);

Console.WriteLine();
Console.WriteLine(overallPassed
    ? $"=== DRILL PASSED ({scenarios.Count} scenarios) -- evidence written to {outputPath} ==="
    : $"=== DRILL FAILED -- evidence written to {outputPath} ===");

return overallPassed ? 0 : 1;

sealed record ScenarioResult(
    string ScenarioName,
    bool Passed,
    bool ExpectedAllowed,
    bool ActualAllowed,
    IReadOnlyList<string> Violations,
    string Details,
    int TelemetryEventCount);

sealed record DrillEvidence(
    string Drill,
    string TenantId,
    DateTime RunAtUtc,
    IReadOnlyList<ScenarioResult> Scenarios,
    bool Passed);

sealed class CapturingLogEventWriter : ILogEventWriter
{
    public List<ILogEvent> Events { get; } = new();

    public void Write(ILogEvent logEvent) => Events.Add(logEvent);
}
