// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.SharedKernel.Contracts.Enterprise
//  Layer:          Tests (Unit)
//  File:           CanonicalModelSchemaDiffTests.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Schema-diff automation tests. Exercises CanonicalModelSchemaDiffer both as a
//                  self-test (it correctly reports additions/removals/changes on synthetic JSON)
//                  and against the real serialized Vn/Vn+1 canonical DTO pairs, asserting every
//                  real pair is additive-only. Isolated, deterministic -- no I/O, no network.
// ============================================================================
using System.Text.Json;
using PlantSafety.SharedKernel.Contracts.Enterprise.CanonicalModel;

namespace PlantSafety.Tests.SharedKernel.Contracts.Enterprise;

/// <summary>Tests for <see cref="CanonicalModelSchemaDiffer"/> and its application to the real canonical DTOs.</summary>
public sealed class CanonicalModelSchemaDiffTests : UnitTestBase
{
    [Fact]
    public void Diff_WithOnlyAddedFields_ShouldReportAdditiveOnly()
    {
        var older = """{"id":1,"name":"a"}""";
        var newer = """{"id":1,"name":"a","extra":true}""";

        var result = CanonicalModelSchemaDiffer.Diff(older, newer);

        result.AddedFields.Should().ContainSingle().Which.Should().Be("extra");
        result.RemovedFields.Should().BeEmpty();
        result.ChangedFields.Should().BeEmpty();
        result.IsAdditiveOnly.Should().BeTrue();
    }

    [Fact]
    public void Diff_WithRemovedField_ShouldReportRemoval_AndNotBeAdditiveOnly()
    {
        var older = """{"id":1,"name":"a"}""";
        var newer = """{"id":1}""";

        var result = CanonicalModelSchemaDiffer.Diff(older, newer);

        result.RemovedFields.Should().ContainSingle().Which.Should().Be("name");
        result.IsAdditiveOnly.Should().BeFalse();
    }

    [Fact]
    public void Diff_WithChangedFieldKind_ShouldReportChange_AndNotBeAdditiveOnly()
    {
        var older = """{"id":1}""";
        var newer = """{"id":"one"}""";

        var result = CanonicalModelSchemaDiffer.Diff(older, newer);

        result.ChangedFields.Should().ContainSingle().Which.Should().Be("id");
        result.IsAdditiveOnly.Should().BeFalse();
    }

    [Fact]
    public void Diff_WithFieldGoingFromNullToPopulated_ShouldNotReportAChange()
    {
        var older = """{"id":1,"note":null}""";
        var newer = """{"id":1,"note":"hello"}""";

        var result = CanonicalModelSchemaDiffer.Diff(older, newer);

        result.ChangedFields.Should().BeEmpty();
        result.IsAdditiveOnly.Should().BeTrue();
    }

    public static TheoryData<string, object, object> RealCanonicalPairs => new()
    {
        { "CanonicalAsset", CanonicalModelSampleData.AssetV1(), CanonicalModelSampleData.AssetV2() },
        { "CanonicalAsset", CanonicalModelSampleData.AssetV2(), CanonicalModelSampleData.AssetV3() },
        { "CanonicalWorkOrder", CanonicalModelSampleData.WorkOrderV1(), CanonicalModelSampleData.WorkOrderV2() },
        { "CanonicalWorkOrder", CanonicalModelSampleData.WorkOrderV2(), CanonicalModelSampleData.WorkOrderV3() },
        { "CanonicalProductionOrder", CanonicalModelSampleData.ProductionOrderV1(), CanonicalModelSampleData.ProductionOrderV2() },
        { "CanonicalProductionOrder", CanonicalModelSampleData.ProductionOrderV2(), CanonicalModelSampleData.ProductionOrderV3() },
        { "CanonicalRiskEvent", CanonicalModelSampleData.RiskEventV1(), CanonicalModelSampleData.RiskEventV2() },
        { "CanonicalRiskEvent", CanonicalModelSampleData.RiskEventV2(), CanonicalModelSampleData.RiskEventV3() },
        { "CanonicalImprovementAction", CanonicalModelSampleData.ImprovementActionV1(), CanonicalModelSampleData.ImprovementActionV2() },
        { "CanonicalImprovementAction", CanonicalModelSampleData.ImprovementActionV2(), CanonicalModelSampleData.ImprovementActionV3() },
        { "CanonicalSchedulingBlock", CanonicalModelSampleData.SchedulingBlockV1(), CanonicalModelSampleData.SchedulingBlockV2() },
        { "CanonicalSchedulingBlock", CanonicalModelSampleData.SchedulingBlockV2(), CanonicalModelSampleData.SchedulingBlockV3() },
    };

    [Theory]
    [MemberData(nameof(RealCanonicalPairs))]
    public void RealCanonicalVnPair_ShouldDiffAsAdditiveOnly(string conceptName, object olderSample, object newerSample)
    {
        var olderJson = JsonSerializer.Serialize(olderSample, olderSample.GetType(), CanonicalModelJsonOptions.Value);
        var newerJson = JsonSerializer.Serialize(newerSample, newerSample.GetType(), CanonicalModelJsonOptions.Value);

        var result = CanonicalModelSchemaDiffer.Diff(olderJson, newerJson);

        result.IsAdditiveOnly.Should().BeTrue(
            because: $"{conceptName}'s next version must only add fields; " +
                     $"removed=[{string.Join(",", result.RemovedFields)}], changed=[{string.Join(",", result.ChangedFields)}]");
    }
}
