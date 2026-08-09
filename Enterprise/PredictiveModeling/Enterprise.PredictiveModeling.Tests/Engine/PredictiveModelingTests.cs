// ============================================================================
//  Project:        Enterprise.PredictiveModeling.Tests
//  File:           PredictiveModelingTests.cs
//  Created:        2026-08-08
//  Description:    Phase D.8 unit tests for EnterprisePredictiveModelEngine:
//                  each of the four forecast methods, verifying correct
//                  delegation to DeterministicSeriesMath (trend direction,
//                  anomaly detection, clamping).
// ============================================================================
using FluentAssertions;

using Enterprise.PredictiveModeling.Abstractions.Engine;
using Enterprise.PredictiveModeling.Abstractions.Forecasts;

using Xunit;

namespace Enterprise.PredictiveModeling.Tests.Engine;

public sealed class PredictiveModelingTests
{
    [Fact]
    public void ForecastRisk_RisingSeries_HasPositiveTrendSlope()
    {
        EnterpriseRiskForecast forecast = EnterprisePredictiveModelEngine.ForecastRisk([1, 2, 3, 4, 5], windowSize: 3);

        forecast.TrendSlope.Should().BePositive();
        forecast.ForecastedNextPeriodRiskWeight.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ForecastRisk_SpikeAtEnd_IsAnomalousSpike()
    {
        EnterpriseRiskForecast forecast = EnterprisePredictiveModelEngine.ForecastRisk([1, 1, 1, 1, 50], windowSize: 4);

        forecast.IsAnomalousSpike.Should().BeTrue();
    }

    [Fact]
    public void ForecastCompliance_ClampsForecastToPercentRange()
    {
        EnterpriseComplianceForecast forecast = EnterprisePredictiveModelEngine.ForecastCompliance([95, 97, 99, 99.9], windowSize: 4);

        forecast.ForecastedNextPeriodCompliancePercent.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void ForecastHazard_FlatHistory_IsNotAnomalous()
    {
        EnterpriseHazardForecast forecast = EnterprisePredictiveModelEngine.ForecastHazard([2, 2, 2, 2, 2], windowSize: 4);

        forecast.IsAnomalousSpike.Should().BeFalse();
    }

    [Fact]
    public void ForecastHazard_SpikeAtEnd_IsAnomalousSpike()
    {
        EnterpriseHazardForecast forecast = EnterprisePredictiveModelEngine.ForecastHazard([0, 0, 0, 0, 20], windowSize: 4);

        forecast.IsAnomalousSpike.Should().BeTrue();
    }

    [Fact]
    public void ForecastSeverity_RisingWeights_IsEscalating()
    {
        EnterpriseSeverityForecast forecast = EnterprisePredictiveModelEngine.ForecastSeverity([1, 1, 2, 3, 4], windowSize: 3);

        forecast.IsEscalating.Should().BeTrue();
    }

    [Fact]
    public void ForecastSeverity_FlatWeights_IsNotEscalating()
    {
        EnterpriseSeverityForecast forecast = EnterprisePredictiveModelEngine.ForecastSeverity([2, 2, 2, 2], windowSize: 3);

        forecast.IsEscalating.Should().BeFalse();
    }
}
