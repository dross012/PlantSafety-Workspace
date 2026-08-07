// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.PHMSA.Classification
//  Layer:          Tests (Unit)
//  File:           ClassificationEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the ClassificationEngine domain service (PHMSA Hazmat
//                  Classification subdomain). Covers a successful classification of a known
//                  material (acetone), an unsuccessful classification of an unknown material,
//                  and a TIH material classification (chlorine).
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Services;

namespace PlantSafety.Tests.Compliance.PHMSA.Classification;

public sealed class ClassificationEngineTests : UnitTestBase
{
    private static readonly ClassificationEngine Engine = new();

    [Fact]
    public void ClassifyMaterial_KnownMaterial_Acetone_ShouldSucceed()
    {
        var result = Engine.ClassifyMaterial(new ClassificationInput(
            CasNumber: "67-64-1",
            WasteProfileCode: null,
            SdsHazardStatement: null,
            MaterialName: "Acetone"));

        result.Success.Should().BeTrue();
        result.UNNumber!.Value.Value.Should().Be("UN1090");
        result.ProperShippingName!.Value.Value.Should().Be("Acetone");
        result.HazardClass!.Value.ToString().Should().Be("3");
        result.PackingGroup.Should().Be(PackingGroup.II);
        result.IsTIH.Should().BeFalse();
        result.TIHZone.Should().BeNull();
    }

    [Fact]
    public void ClassifyMaterial_UnknownCasNumber_ShouldFail()
    {
        var result = Engine.ClassifyMaterial(new ClassificationInput(
            CasNumber: "0-00-0",
            WasteProfileCode: null,
            SdsHazardStatement: null,
            MaterialName: "Totally Unknown Substance"));

        result.Success.Should().BeFalse();
        result.UNNumber.Should().BeNull();
        result.Messages.Should().ContainSingle(m => m.Contains("No classification found"));
    }

    [Fact]
    public void ClassifyMaterial_MissingCasNumber_ShouldFail()
    {
        var result = Engine.ClassifyMaterial(new ClassificationInput(
            CasNumber: null,
            WasteProfileCode: null,
            SdsHazardStatement: null,
            MaterialName: "Mystery Liquid"));

        result.Success.Should().BeFalse();
        result.Messages.Should().NotBeEmpty();
    }

    [Fact]
    public void ClassifyMaterial_Chlorine_ShouldPopulateTIHZone()
    {
        var result = Engine.ClassifyMaterial(new ClassificationInput(
            CasNumber: "7782-50-5",
            WasteProfileCode: null,
            SdsHazardStatement: null,
            MaterialName: "Chlorine"));

        result.Success.Should().BeTrue();
        result.IsTIH.Should().BeTrue();
        result.TIHZone.Should().Be(TIHZone.B);
        result.UNNumber!.Value.Value.Should().Be("UN1017");
        result.IsMarinePollutant.Should().BeTrue();
    }

    [Fact]
    public void ClassifyMaterial_SulfuricAcid_ShouldClassifyAsCorrosivePackingGroupII()
    {
        var result = Engine.ClassifyMaterial(new ClassificationInput(
            CasNumber: "7664-93-9",
            WasteProfileCode: null,
            SdsHazardStatement: null,
            MaterialName: "Sulfuric Acid"));

        result.Success.Should().BeTrue();
        result.HazardClass!.Value.ToString().Should().Be("8");
        result.PackingGroup.Should().Be(PackingGroup.II);
    }
}
