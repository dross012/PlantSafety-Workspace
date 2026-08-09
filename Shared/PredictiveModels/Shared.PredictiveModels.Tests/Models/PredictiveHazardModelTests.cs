// ============================================================================
//  Project:        Shared.PredictiveModels.Tests
//  File:           PredictiveHazardModelTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.11 unit tests for DeterministicSeriesMath and
//                  every predictive model: moving average correctness,
//                  trend slope direction, and z-score anomaly detection.
// ============================================================================
using FluentAssertions;

using Shared.PredictiveModels.Abstractions.Models;
using Shared.PredictiveModels.Abstractions.Statistics;

using Xunit;

namespace Shared.PredictiveModels.Tests.Models;

public sealed class PredictiveHazardModelTests
{
    [Fact]
    public void DeterministicSeriesMath_MovingAverage_UsesLastWindowSizeValues()
    {
        double result = DeterministicSeriesMath.MovingAverage([1, 2, 3, 4, 10], windowSize: 2);

        result.Should().Be(7d);
    }

    [Fact]
    public void DeterministicSeriesMath_MovingAverage_WindowLargerThanSeries_UsesWholeSeries()
    {
        double result = DeterministicSeriesMath.MovingAverage([2, 4, 6], windowSize: 10);

        result.Should().Be(4d);
    }

    [Fact]
    public void DeterministicSeriesMath_TrendSlope_RisingSeries_IsPositive()
    {
        double slope = DeterministicSeriesMath.TrendSlope([1, 2, 3, 4, 5]);

        slope.Should().BePositive();
    }

    [Fact]
    public void DeterministicSeriesMath_TrendSlope_FlatSeries_IsZero()
    {
        double slope = DeterministicSeriesMath.TrendSlope([5, 5, 5, 5]);

        slope.Should().Be(0d);
    }

    [Fact]
    public void DeterministicSeriesMath_TrendSlope_SingleValue_ReturnsZero()
    {
        DeterministicSeriesMath.TrendSlope([5]).Should().Be(0d);
    }

    [Fact]
    public void DeterministicSeriesMath_ZScore_LatestFarAboveMean_IsHighlyPositive()
    {
        double zScore = DeterministicSeriesMath.ZScore([1, 1, 1, 1], latestValue: 20);

        zScore.Should().BePositive();
        DeterministicSeriesMath.IsAnomalous(zScore).Should().BeTrue();
    }

    [Fact]
    public void DeterministicSeriesMath_ZScore_LatestNearMean_IsNotAnomalous()
    {
        double zScore = DeterministicSeriesMath.ZScore([2, 3, 4, 5, 6], latestValue: 4);

        DeterministicSeriesMath.IsAnomalous(zScore).Should().BeFalse();
    }

    [Fact]
    public void HazardLikelihoodModel_Predict_RisingCounts_PredictsAboveAverage()
    {
        HazardLikelihoodPrediction prediction = HazardLikelihoodModel.Predict([1, 2, 3, 4, 5], windowSize: 3);

        prediction.PredictedNextPeriodCount.Should().BeGreaterThan(prediction.MovingAverage - 0.01);
        prediction.TrendSlope.Should().BePositive();
    }

    [Fact]
    public void SeverityLikelihoodModel_Predict_RisingWeights_IsEscalating()
    {
        SeverityLikelihoodPrediction prediction = SeverityLikelihoodModel.Predict([1, 1, 2, 3, 4], windowSize: 3);

        prediction.IsEscalating.Should().BeTrue();
    }

    [Fact]
    public void SeverityLikelihoodModel_Predict_FlatWeights_IsNotEscalating()
    {
        SeverityLikelihoodPrediction prediction = SeverityLikelihoodModel.Predict([2, 2, 2, 2], windowSize: 3);

        prediction.IsEscalating.Should().BeFalse();
    }

    [Fact]
    public void ExposureLikelihoodModel_Predict_LatestSpike_IsAnomalous()
    {
        ExposureLikelihoodPrediction prediction = ExposureLikelihoodModel.Predict([1, 1, 1, 1], latestPeriodExposureCount: 20, windowSize: 4);

        prediction.IsAnomalous.Should().BeTrue();
    }

    [Fact]
    public void ExposureLikelihoodModel_Predict_NoPriorHistory_ReturnsZeroAverageAndNoAnomaly()
    {
        ExposureLikelihoodPrediction prediction = ExposureLikelihoodModel.Predict([], latestPeriodExposureCount: 3, windowSize: 4);

        prediction.MovingAverage.Should().Be(0d);
        prediction.IsAnomalous.Should().BeFalse();
    }

    [Fact]
    public void MachineFailurePredictionModel_Predict_SpikeAtEnd_IsAnomalousSpike()
    {
        MachineFailurePrediction prediction = MachineFailurePredictionModel.Predict([0, 0, 0, 0, 15], windowSize: 4);

        prediction.IsAnomalousSpike.Should().BeTrue();
        prediction.PredictedNextPeriodFailureCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void EmployeeInjuryPredictionModel_Predict_SpikeAtEnd_IsAnomalousSpike()
    {
        EmployeeInjuryPrediction prediction = EmployeeInjuryPredictionModel.Predict([0, 0, 0, 0, 12], windowSize: 4);

        prediction.IsAnomalousSpike.Should().BeTrue();
    }

    [Fact]
    public void EmployeeInjuryPredictionModel_Predict_SteadyLowCounts_IsNotAnomalous()
    {
        EmployeeInjuryPrediction prediction = EmployeeInjuryPredictionModel.Predict([1, 1, 1, 1, 1], windowSize: 4);

        prediction.IsAnomalousSpike.Should().BeFalse();
    }
}
