// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.MachineSafety.CraneHoistSystems.BackwardCompatibility
//  Layer:          Tests (Backward Compatibility)
//  File:           CraneHoistSystemsBackwardCompatibilityTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Guards the numeric ordinals of CraneHoistSystemsStatus and the bit positions
//                  of the [Flags] CraneHoistAssetType enum against accidental renumbering.
//                  Both enums are persisted to SQL as their NVARCHAR .ToString() representation
//                  (see CraneHoistSystemsDapperRepository / usp_CraneHoistSystems_Create.sql), so
//                  renumbering ordinals is actually safe for storage -- but application code,
//                  in-flight domain events, and any client caching numeric values are not.
//                  These tests pin the documented "Additive-only evolution" contract stated in
//                  every CraneHoistSystems file header. Isolated, deterministic, <50ms per test.
// ============================================================================
using PlantSafety.Domain.MachineSafety.CraneHoistSystems.Enums;

namespace PlantSafety.Tests.MachineSafety.CraneHoistSystems.BackwardCompatibility;

public sealed class CraneHoistSystemsBackwardCompatibilityTests : UnitTestBase
{
    [Theory]
    [InlineData(CraneHoistSystemsStatus.Registered, 0)]
    [InlineData(CraneHoistSystemsStatus.InService, 1)]
    [InlineData(CraneHoistSystemsStatus.InspectionDue, 2)]
    [InlineData(CraneHoistSystemsStatus.InspectionOverdue, 3)]
    [InlineData(CraneHoistSystemsStatus.Suspended, 4)]
    [InlineData(CraneHoistSystemsStatus.Retired, 5)]
    public void CraneHoistSystemsStatus_OrdinalsRemainStable(CraneHoistSystemsStatus status, int expectedOrdinal)
    {
        ((int)status).Should().Be(expectedOrdinal, "existing persisted/serialized data and in-flight events depend on stable ordinals");
    }

    [Theory]
    [InlineData(CraneHoistAssetType.None, 0)]
    [InlineData(CraneHoistAssetType.OverheadBridgeCrane, 1)]
    [InlineData(CraneHoistAssetType.GantryCrane, 2)]
    [InlineData(CraneHoistAssetType.JibCrane, 4)]
    [InlineData(CraneHoistAssetType.MobileCrane, 8)]
    [InlineData(CraneHoistAssetType.Hoist, 16)]
    public void CraneHoistAssetType_BitPositionsRemainStable(CraneHoistAssetType assetType, int expectedValue)
    {
        ((int)assetType).Should().Be(expectedValue, "the [Flags] bit positions are additive-only per the file header contract");
    }

    [Fact]
    public void CraneHoistSystemsStatus_NoValuesHaveBeenRemoved()
    {
        var definedValues = Enum.GetValues<CraneHoistSystemsStatus>();
        definedValues.Should().Contain(new[]
        {
            CraneHoistSystemsStatus.Registered,
            CraneHoistSystemsStatus.InService,
            CraneHoistSystemsStatus.InspectionDue,
            CraneHoistSystemsStatus.InspectionOverdue,
            CraneHoistSystemsStatus.Suspended,
            CraneHoistSystemsStatus.Retired,
        });
    }

    [Fact]
    public void CraneHoistAssetType_NoValuesHaveBeenRemoved()
    {
        var definedValues = Enum.GetValues<CraneHoistAssetType>();
        definedValues.Should().Contain(new[]
        {
            CraneHoistAssetType.None,
            CraneHoistAssetType.OverheadBridgeCrane,
            CraneHoistAssetType.GantryCrane,
            CraneHoistAssetType.JibCrane,
            CraneHoistAssetType.MobileCrane,
            CraneHoistAssetType.Hoist,
        });
    }

    [Fact]
    public void CraneHoistSystemsStatus_ToStringNamesUsedForPersistence_RemainStable()
    {
        // Status is persisted as NVARCHAR(100) via .ToString() (see usp_CraneHoistSystems_Create.sql /
        // CraneHoistSystemsDapperRepository) -- renaming an enum member silently orphans existing rows.
        CraneHoistSystemsStatus.Registered.ToString().Should().Be("Registered");
        CraneHoistSystemsStatus.InService.ToString().Should().Be("InService");
        CraneHoistSystemsStatus.InspectionDue.ToString().Should().Be("InspectionDue");
        CraneHoistSystemsStatus.InspectionOverdue.ToString().Should().Be("InspectionOverdue");
        CraneHoistSystemsStatus.Suspended.ToString().Should().Be("Suspended");
        CraneHoistSystemsStatus.Retired.ToString().Should().Be("Retired");
    }
}
