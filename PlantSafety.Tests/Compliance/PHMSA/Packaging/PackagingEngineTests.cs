// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.PHMSA.Packaging
//  Layer:          Tests (Unit)
//  File:           PackagingEngineTests.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the PackagingEngine domain service (PHMSA Packaging
//                  Compliance subdomain). Covers a packaging determination for a Class 8 PG II
//                  material, and packaging validation accepting/rejecting proposed specs.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.Services;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects;

namespace PlantSafety.Tests.Compliance.PHMSA.Packaging;

public sealed class PackagingEngineTests : UnitTestBase
{
    private static readonly PackagingEngine Engine = new();

    [Fact]
    public void DeterminePackaging_Class8PackingGroupII_ShouldReturnAuthorizedSpecs()
    {
        var result = Engine.DeterminePackaging(new PackagingDeterminationInput(
            HazardClass: HazardClass.Corrosive,
            PackingGroup: PackingGroup.II,
            Quantity: 200m,
            QuantityUnit: "L",
            ContainerTypeRequested: "Drum"));

        result.Success.Should().BeTrue();
        result.AuthorizedPackaging.Should().NotBeEmpty();
        result.AuthorizedPackaging.Should().Contain(spec => spec.Value == "1A2");
        result.ClosureInstructions.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void DeterminePackaging_UnknownCombination_ShouldFail()
    {
        var result = Engine.DeterminePackaging(new PackagingDeterminationInput(
            HazardClass: HazardClass.InfectiousSubstance,
            PackingGroup: PackingGroup.I,
            Quantity: 10m,
            QuantityUnit: "kg",
            ContainerTypeRequested: "Box"));

        result.Success.Should().BeFalse();
        result.AuthorizedPackaging.Should().BeEmpty();
        result.Messages.Should().ContainSingle(m => m.Contains("No packaging determination rule found"));
    }

    [Fact]
    public void DeterminePackaging_NonPositiveQuantity_ShouldFail()
    {
        var result = Engine.DeterminePackaging(new PackagingDeterminationInput(
            HazardClass: HazardClass.Corrosive,
            PackingGroup: PackingGroup.II,
            Quantity: 0m,
            QuantityUnit: "L",
            ContainerTypeRequested: "Drum"));

        result.Success.Should().BeFalse();
    }

    [Fact]
    public void ValidatePackaging_AuthorizedSpec_ShouldReturnIsAuthorizedTrue()
    {
        var result = Engine.ValidatePackaging(new DOTSpec("1A2"), HazardClass.Corrosive, PackingGroup.II);

        result.IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void ValidatePackaging_UnauthorizedSpec_ShouldRejectWithMessage()
    {
        // "4G" (fiberboard box) is not authorized for Class 8 PG II corrosive liquids in the
        // illustrative rules table -- only steel/composite specs are.
        var result = Engine.ValidatePackaging(new DOTSpec("4G"), HazardClass.Corrosive, PackingGroup.II);

        result.IsAuthorized.Should().BeFalse();
        result.Message.Should().Contain("NOT authorized");
    }
}
