// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.PHMSA.ShippingPapers
//  Layer:          Tests (Unit)
//  File:           ShippingPaperGeneratorTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the ShippingPaperGenerator domain service.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Enums;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Services;

namespace PlantSafety.Tests.Compliance.PHMSA.ShippingPapers;

/// <summary>Unit tests for <see cref="ShippingPaperGenerator"/>.</summary>
public sealed class ShippingPaperGeneratorTests : UnitTestBase
{
    private readonly ShippingPaperGenerator _generator = new();

    [Fact]
    public void BuildShippingPaper_MultipleMaterials_ShouldIncludeAllInBasicDescriptionSequence()
    {
        var input = new ShippingPaperInput(
            Materials:
            [
                (new UNNumber("UN1090"), new ProperShippingName("Acetone"), HazardClass.FlammableLiquid, PackingGroup.II),
                (new UNNumber("UN1830"), new ProperShippingName("Sulfuric acid"), HazardClass.Corrosive, PackingGroup.II),
            ],
            Mode: TransportMode.Ground,
            WasteManifestReference: null,
            ShipperCertificationText: "Certified.");

        var result = _generator.BuildShippingPaper(input);

        result.Success.Should().BeTrue();
        result.ShippingPaper.Should().NotBeNull();
        result.ShippingPaper!.BasicDescriptionSequence.Should().Contain("UN1090, Acetone, 3, PG II");
        result.ShippingPaper!.BasicDescriptionSequence.Should().Contain("UN1830, Sulfuric acid, 8, PG II");
    }

    [Fact]
    public void BuildShippingPaper_AirMode_ShouldIncludeIataAdditionalRequirement()
    {
        var input = new ShippingPaperInput(
            Materials: [(new UNNumber("UN1090"), new ProperShippingName("Acetone"), HazardClass.FlammableLiquid, PackingGroup.II)],
            Mode: TransportMode.Air,
            WasteManifestReference: null,
            ShipperCertificationText: "Certified.");

        var result = _generator.BuildShippingPaper(input);

        result.Success.Should().BeTrue();
        result.ShippingPaper!.AdditionalModalRequirements.Should()
            .Contain(m => m.Contains("IATA", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void BuildShippingPaper_GroundMode_ShouldHaveNoAdditionalRequirements()
    {
        var input = new ShippingPaperInput(
            Materials: [(new UNNumber("UN1090"), new ProperShippingName("Acetone"), HazardClass.FlammableLiquid, PackingGroup.II)],
            Mode: TransportMode.Ground,
            WasteManifestReference: null,
            ShipperCertificationText: "Certified.");

        var result = _generator.BuildShippingPaper(input);

        result.ShippingPaper!.AdditionalModalRequirements.Should().BeEmpty();
    }

    [Fact]
    public void BuildShippingPaper_NoMaterials_ShouldFail()
    {
        var input = new ShippingPaperInput(
            Materials: [],
            Mode: TransportMode.Ground,
            WasteManifestReference: null,
            ShipperCertificationText: "Certified.");

        var result = _generator.BuildShippingPaper(input);

        result.Success.Should().BeFalse();
        result.ShippingPaper.Should().BeNull();
    }

    [Fact]
    public void BuildShippingPaper_WithWasteManifestReference_ShouldPreserveIt()
    {
        var input = new ShippingPaperInput(
            Materials: [(new UNNumber("UN1090"), new ProperShippingName("Acetone"), HazardClass.FlammableLiquid, PackingGroup.II)],
            Mode: TransportMode.Ground,
            WasteManifestReference: "MANIFEST-12345",
            ShipperCertificationText: "Certified.");

        var result = _generator.BuildShippingPaper(input);

        result.ShippingPaper!.WasteManifestReference.Should().Be("MANIFEST-12345");
    }
}
