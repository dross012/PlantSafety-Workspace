// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Sustainability.AdvancedMetrics.Domain
//  Layer:          Tests (Unit)
//  File:           ChildRecordInvariantTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Construction-time invariant tests for the four immutable child record entities
//                  of AdvancedMetricsLedger: CarbonEmissionRecord, EnergyIntensityMetric,
//                  WaterIntensityMetric, WasteDiversionRecord. Covers negative-value guards,
//                  divide-by-zero guards, methodology-version requirement, and the computed
//                  intensity/diversion-rate arithmetic. Isolated, deterministic, <50ms per test.
//                  No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Entities;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;

namespace PlantSafety.Tests.Sustainability.AdvancedMetrics.Domain;

public sealed class ChildRecordInvariantTests : UnitTestBase
{
    // ---- CarbonEmissionRecord -------------------------------------------------

    [Fact]
    public void CarbonEmissionRecord_WithValidArguments_TrimsDescriptionAndMethodology()
    {
        var record = new CarbonEmissionRecord(
            CarbonEmissionRecordId.New(), GhgScope.Scope2, "  Grid electricity  ", 42.5m, "  GHG-Protocol-2026.1  ",
            FixedTimestamp, FixedTimestamp, TestUserId);

        record.ActivityDescription.Should().Be("Grid electricity");
        record.MethodologyVersion.Should().Be("GHG-Protocol-2026.1");
        record.EmissionsMetricTonsCo2e.Should().Be(42.5m);
    }

    [Fact]
    public void CarbonEmissionRecord_WithNegativeEmissions_Throws()
    {
        var act = () => new CarbonEmissionRecord(
            CarbonEmissionRecordId.New(), GhgScope.Scope1, "x", -1m, "v1", FixedTimestamp, FixedTimestamp, TestUserId);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("emissionsMetricTonsCo2e");
    }

    [Fact]
    public void CarbonEmissionRecord_WithBlankMethodologyVersion_Throws()
    {
        var act = () => new CarbonEmissionRecord(
            CarbonEmissionRecordId.New(), GhgScope.Scope1, "x", 1m, "   ", FixedTimestamp, FixedTimestamp, TestUserId);

        act.Should().Throw<ArgumentException>().WithParameterName("methodologyVersion");
    }

    // ---- EnergyIntensityMetric -------------------------------------------------

    [Fact]
    public void EnergyIntensityMetric_ComputesIntensityFromConsumptionAndOutput()
    {
        var metric = new EnergyIntensityMetric(
            EnergyIntensityMetricId.New(), energyConsumedKwh: 1000m, outputUnits: 400m, "v1",
            FixedTimestamp, FixedTimestamp, TestUserId);

        metric.IntensityValue.Should().Be(2.5m);
    }

    [Fact]
    public void EnergyIntensityMetric_WithNegativeConsumption_Throws()
    {
        var act = () => new EnergyIntensityMetric(
            EnergyIntensityMetricId.New(), -1m, 1m, "v1", FixedTimestamp, FixedTimestamp, TestUserId);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("energyConsumedKwh");
    }

    [Fact]
    public void EnergyIntensityMetric_WithZeroOutputUnits_Throws()
    {
        var act = () => new EnergyIntensityMetric(
            EnergyIntensityMetricId.New(), 100m, 0m, "v1", FixedTimestamp, FixedTimestamp, TestUserId);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("outputUnits");
    }

    // ---- WaterIntensityMetric -------------------------------------------------

    [Fact]
    public void WaterIntensityMetric_ComputesIntensityFromConsumptionAndOutput()
    {
        var metric = new WaterIntensityMetric(
            WaterIntensityMetricId.New(), waterConsumedCubicMeters: 300m, outputUnits: 500m, "v1",
            FixedTimestamp, FixedTimestamp, TestUserId);

        metric.IntensityValue.Should().Be(0.6m);
    }

    [Fact]
    public void WaterIntensityMetric_WithNegativeConsumption_Throws()
    {
        var act = () => new WaterIntensityMetric(
            WaterIntensityMetricId.New(), -1m, 1m, "v1", FixedTimestamp, FixedTimestamp, TestUserId);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("waterConsumedCubicMeters");
    }

    [Fact]
    public void WaterIntensityMetric_WithZeroOutputUnits_Throws()
    {
        var act = () => new WaterIntensityMetric(
            WaterIntensityMetricId.New(), 100m, 0m, "v1", FixedTimestamp, FixedTimestamp, TestUserId);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("outputUnits");
    }

    // ---- WasteDiversionRecord -------------------------------------------------

    [Fact]
    public void WasteDiversionRecord_ComputesDiversionRatePercent()
    {
        var record = new WasteDiversionRecord(
            WasteDiversionRecordId.New(), totalWasteKg: 1000m, divertedWasteKg: 750m, "v1",
            FixedTimestamp, FixedTimestamp, TestUserId);

        record.DiversionRatePercent.Should().Be(75.00m);
    }

    [Fact]
    public void WasteDiversionRecord_WithZeroTotalWaste_ReturnsZeroDiversionRateWithoutDivideByZero()
    {
        var record = new WasteDiversionRecord(
            WasteDiversionRecordId.New(), totalWasteKg: 0m, divertedWasteKg: 0m, "v1",
            FixedTimestamp, FixedTimestamp, TestUserId);

        record.DiversionRatePercent.Should().Be(0m);
    }

    [Fact]
    public void WasteDiversionRecord_WithNegativeTotalWaste_Throws()
    {
        var act = () => new WasteDiversionRecord(
            WasteDiversionRecordId.New(), -1m, 0m, "v1", FixedTimestamp, FixedTimestamp, TestUserId);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("totalWasteKg");
    }

    [Fact]
    public void WasteDiversionRecord_WithDivertedExceedingTotal_Throws()
    {
        var act = () => new WasteDiversionRecord(
            WasteDiversionRecordId.New(), totalWasteKg: 100m, divertedWasteKg: 150m, "v1",
            FixedTimestamp, FixedTimestamp, TestUserId);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("divertedWasteKg");
    }

    [Fact]
    public void WasteDiversionRecord_WithNegativeDivertedWaste_Throws()
    {
        var act = () => new WasteDiversionRecord(
            WasteDiversionRecordId.New(), totalWasteKg: 100m, divertedWasteKg: -1m, "v1",
            FixedTimestamp, FixedTimestamp, TestUserId);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("divertedWasteKg");
    }
}
