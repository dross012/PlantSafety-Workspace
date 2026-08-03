// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SharedKernel.Contracts.Enterprise
//  Layer:          Tests (Unit)
//  File:           CanonicalModelSnapshotTests.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Lightweight, in-house JSON-schema-snapshot contract tests for the enterprise
//                  canonical model. No contract-testing library (e.g. Pact.NET) is referenced
//                  anywhere in this codebase, so this suite implements the "serialize -> snapshot ->
//                  diff" approach directly: each canonical DTO version is serialized to indented
//                  JSON and compared against a checked-in baseline file under
//                  PlantSafety.Tests/SharedKernel/Contracts/Enterprise/Snapshots/. A silent field
//                  addition/removal/rename that is not accompanied by an explicit snapshot update
//                  (git diff review) fails this suite. Isolated, deterministic -- no network.
// ============================================================================
using System.Runtime.CompilerServices;
using System.Text.Json;
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;

namespace PlantSafety.Tests.SharedKernel.Contracts.Enterprise;

/// <summary>JSON-schema-snapshot contract tests for every enterprise canonical DTO version.</summary>
public sealed class CanonicalModelSnapshotTests : UnitTestBase
{
    private static string SnapshotsDirectory([CallerFilePath] string thisFilePath = "") =>
        Path.Combine(Path.GetDirectoryName(thisFilePath)!, "Snapshots");

    public static TheoryData<string, object> CanonicalSamples => new()
    {
        { "CanonicalAssetV1", CanonicalModelSampleData.AssetV1() },
        { "CanonicalAssetV2", CanonicalModelSampleData.AssetV2() },
        { "CanonicalAssetV3", CanonicalModelSampleData.AssetV3() },
        { "CanonicalWorkOrderV1", CanonicalModelSampleData.WorkOrderV1() },
        { "CanonicalWorkOrderV2", CanonicalModelSampleData.WorkOrderV2() },
        { "CanonicalWorkOrderV3", CanonicalModelSampleData.WorkOrderV3() },
        { "CanonicalProductionOrderV1", CanonicalModelSampleData.ProductionOrderV1() },
        { "CanonicalProductionOrderV2", CanonicalModelSampleData.ProductionOrderV2() },
        { "CanonicalProductionOrderV3", CanonicalModelSampleData.ProductionOrderV3() },
        { "CanonicalRiskEventV1", CanonicalModelSampleData.RiskEventV1() },
        { "CanonicalRiskEventV2", CanonicalModelSampleData.RiskEventV2() },
        { "CanonicalRiskEventV3", CanonicalModelSampleData.RiskEventV3() },
        { "CanonicalImprovementActionV1", CanonicalModelSampleData.ImprovementActionV1() },
        { "CanonicalImprovementActionV2", CanonicalModelSampleData.ImprovementActionV2() },
        { "CanonicalImprovementActionV3", CanonicalModelSampleData.ImprovementActionV3() },
        { "CanonicalSchedulingBlockV1", CanonicalModelSampleData.SchedulingBlockV1() },
        { "CanonicalSchedulingBlockV2", CanonicalModelSampleData.SchedulingBlockV2() },
        { "CanonicalSchedulingBlockV3", CanonicalModelSampleData.SchedulingBlockV3() },
    };

    [Theory]
    [MemberData(nameof(CanonicalSamples))]
    public void Serialized_ShouldMatchCheckedInSnapshot(string snapshotName, object sample)
    {
        var actualJson = JsonSerializer.Serialize(sample, sample.GetType(), CanonicalModelJsonOptions.Value);
        var snapshotPath = Path.Combine(SnapshotsDirectory(), $"{snapshotName}.json");

        if (!File.Exists(snapshotPath))
        {
            // First run for this DTO version: record the baseline. Subsequent runs compare against
            // it, so any silent shape change must be reviewed (git diff) and re-committed here.
            File.WriteAllText(snapshotPath, actualJson);
            return;
        }

        var expectedJson = File.ReadAllText(snapshotPath);

        actualJson.Should().Be(expectedJson,
            because: $"{snapshotName}'s serialized shape must not silently change -- if this is an " +
                      $"intentional, additive V(n) change, update {snapshotPath} in the same commit.");
    }
}
