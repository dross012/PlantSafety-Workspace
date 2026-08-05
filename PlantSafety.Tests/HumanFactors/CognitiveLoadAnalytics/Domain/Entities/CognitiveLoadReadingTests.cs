// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Entities
//  Layer:          Tests (Unit)
//  File:           CognitiveLoadReadingTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Invariant tests for the CognitiveLoadReading child entity constructor.
//                   Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Entities;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Entities;

public sealed class CognitiveLoadReadingTests : UnitTestBase
{
    private static CognitiveLoadReading Build(decimal score = 50m, string method = "NASA-TLX") =>
        new(CognitiveLoadReadingId.New(), TestUserId, null, score, CognitiveWorkloadLevel.Moderate,
            method, FixedTimestamp, FixedTimestamp, TestUserId);

    [Fact]
    public void Constructor_WithValidInputs_SetsAllProperties()
    {
        var reading = Build();

        reading.CognitiveLoadScore.Should().Be(50m);
        reading.MeasurementMethod.Should().Be("NASA-TLX");
        reading.WorkerUserId.Should().Be(TestUserId);
        reading.RecordedBy.Should().Be(TestUserId);
    }

    [Fact]
    public void Constructor_TrimsMeasurementMethod()
    {
        var reading = Build(method: "  SelfReport  ");

        reading.MeasurementMethod.Should().Be("SelfReport");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Constructor_WithCognitiveLoadScoreOutOfRange_ThrowsArgumentOutOfRangeException(decimal score)
    {
        var act = () => Build(score);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("cognitiveLoadScore");
    }

    [Fact]
    public void Constructor_WithBlankMeasurementMethod_ThrowsArgumentException()
    {
        var act = () => Build(method: "   ");

        act.Should().Throw<ArgumentException>().WithParameterName("measurementMethod");
    }

    [Fact]
    public void IsOverload_WhenWorkloadLevelIsOverload_ReturnsTrue()
    {
        var reading = new CognitiveLoadReading(
            CognitiveLoadReadingId.New(), TestUserId, null, 90m, CognitiveWorkloadLevel.Overload,
            "NASA-TLX", FixedTimestamp, FixedTimestamp, TestUserId);

        reading.IsOverload.Should().BeTrue();
    }

    [Fact]
    public void IsOverload_WhenWorkloadLevelIsNotOverload_ReturnsFalse()
    {
        var reading = Build();

        reading.IsOverload.Should().BeFalse();
    }
}
