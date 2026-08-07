// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.PHMSA.Segregation
//  Layer:          Tests (Unit)
//  File:           SegregationEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the SegregationEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Segregation.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Segregation.Services;

namespace PlantSafety.Tests.Compliance.PHMSA.Segregation;

/// <summary>Unit tests for <see cref="SegregationEngine"/>.</summary>
public sealed class SegregationEngineTests : UnitTestBase
{
    private readonly SegregationEngine _engine = new();

    [Fact]
    public void EvaluateSegregation_Class1AndClass3_ShouldReportProhibitedViolation()
    {
        var input = new SegregationEvaluationInput(
            LoadItems:
            [
                (HazmatMaterialId.New(), HazardClass.Explosives1_1),
                (HazmatMaterialId.New(), HazardClass.FlammableLiquid),
            ]);

        var result = _engine.EvaluateSegregation(input);

        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.Severity == SegregationSeverity.Prohibited);
        result.RecommendedLoadPlan.Should().NotBeEmpty();
    }

    [Fact]
    public void EvaluateSegregation_CompliantMixedLoad_ShouldReportNoViolations()
    {
        var input = new SegregationEvaluationInput(
            LoadItems:
            [
                (HazmatMaterialId.New(), HazardClass.FlammableLiquid),
                (HazmatMaterialId.New(), HazardClass.Miscellaneous),
            ]);

        var result = _engine.EvaluateSegregation(input);

        result.IsCompliant.Should().BeTrue();
        result.Violations.Should().BeEmpty();
        result.RecommendedLoadPlan.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateSegregation_SameClassPresentTwice_ShouldNotFlagAsViolation()
    {
        var input = new SegregationEvaluationInput(
            LoadItems:
            [
                (HazmatMaterialId.New(), HazardClass.FlammableLiquid),
                (HazmatMaterialId.New(), HazardClass.FlammableLiquid),
            ]);

        var result = _engine.EvaluateSegregation(input);

        result.IsCompliant.Should().BeTrue();
    }

    [Fact]
    public void EvaluateSegregation_NoLoadItems_ShouldBeCompliant()
    {
        var input = new SegregationEvaluationInput(LoadItems: []);

        var result = _engine.EvaluateSegregation(input);

        result.IsCompliant.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }
}
