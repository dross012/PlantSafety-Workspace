// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.PHMSA.Placarding
//  Layer:          Tests (Unit)
//  File:           PlacardingEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the PlacardingEngine domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Services;

namespace PlantSafety.Tests.Compliance.PHMSA.Placarding;

/// <summary>Unit tests for <see cref="PlacardingEngine"/>.</summary>
public sealed class PlacardingEngineTests : UnitTestBase
{
    private readonly PlacardingEngine _engine = new();

    [Fact]
    public void GeneratePlacards_BulkPackaging_ShouldRequirePlacardForPresentClass()
    {
        var input = new PlacardingInput(
            Materials: [(HazardClass.FlammableLiquid, 5000m, "gal")],
            IsBulkPackaging: true);

        var result = _engine.GeneratePlacards(input);

        result.Success.Should().BeTrue();
        result.RequiredPlacards.Should().Contain("FLAMMABLE 3");
    }

    [Fact]
    public void GeneratePlacards_NonBulkSmallQuantity_ShouldRequireNoPlacards()
    {
        var input = new PlacardingInput(
            Materials: [(HazardClass.Corrosive, 50m, "lb")],
            IsBulkPackaging: false);

        var result = _engine.GeneratePlacards(input);

        result.Success.Should().BeTrue();
        result.RequiredPlacards.Should().BeEmpty();
    }

    [Fact]
    public void GeneratePlacards_Table1ClassAnyQuantity_ShouldRequirePlacard()
    {
        var input = new PlacardingInput(
            Materials: [(HazardClass.ToxicGas, 1m, "lb")],
            IsBulkPackaging: false);

        var result = _engine.GeneratePlacards(input);

        result.Success.Should().BeTrue();
        result.RequiredPlacards.Should().Contain("POISON GAS");
    }

    [Fact]
    public void GeneratePlacards_NonBulkTable2AboveThreshold_ShouldRequirePlacard()
    {
        var input = new PlacardingInput(
            Materials: [(HazardClass.Corrosive, 1500m, "lb")],
            IsBulkPackaging: false);

        var result = _engine.GeneratePlacards(input);

        result.Success.Should().BeTrue();
        result.RequiredPlacards.Should().Contain("CORROSIVE 8");
    }

    [Fact]
    public void GeneratePlacards_NonBulkLiquid_ShouldRequireOrientationArrows()
    {
        var input = new PlacardingInput(
            Materials: [(HazardClass.FlammableLiquid, 10m, "L")],
            IsBulkPackaging: false);

        var result = _engine.GeneratePlacards(input);

        result.AnyOrientationArrowsRequired.Should().BeTrue();
    }

    [Fact]
    public void GeneratePlacards_NoMaterials_ShouldFail()
    {
        var input = new PlacardingInput(Materials: [], IsBulkPackaging: false);

        var result = _engine.GeneratePlacards(input);

        result.Success.Should().BeFalse();
        result.Messages.Should().NotBeEmpty();
    }
}
