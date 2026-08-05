// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Entities
//  Layer:          Tests (Unit)
//  File:           WorkloadTrendRecordTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Invariant tests for the WorkloadTrendRecord child entity constructor.
//                   Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Entities;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Domain.Entities;

public sealed class WorkloadTrendRecordTests : UnitTestBase
{
    private static readonly UtcTimestamp PeriodEnd = UtcTimestamp.From(FixedUtcNow.AddHours(8));

    private static WorkloadTrendRecord Build(
        decimal meanScore = 50m, int overloadCount = 1, int readingCount = 10,
        UtcTimestamp? periodStart = null, UtcTimestamp? periodEnd = null) =>
        new(WorkloadTrendRecordId.New(), periodStart ?? FixedTimestamp, periodEnd ?? PeriodEnd,
            meanScore, overloadCount, readingCount, FixedTimestamp, TestUserId);

    [Fact]
    public void Constructor_WithValidInputs_SetsAllProperties()
    {
        var record = Build();

        record.MeanCognitiveLoadScore.Should().Be(50m);
        record.OverloadEventCount.Should().Be(1);
        record.ReadingCount.Should().Be(10);
        record.PeriodStartUtc.Should().Be(FixedTimestamp);
        record.PeriodEndUtc.Should().Be(PeriodEnd);
    }

    [Fact]
    public void Constructor_WithPeriodEndAtOrBeforePeriodStart_ThrowsArgumentException()
    {
        var act = () => Build(periodStart: PeriodEnd, periodEnd: FixedTimestamp);

        act.Should().Throw<ArgumentException>().WithParameterName("periodEndUtc");
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Constructor_WithMeanScoreOutOfRange_ThrowsArgumentOutOfRangeException(decimal meanScore)
    {
        var act = () => Build(meanScore: meanScore);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("meanCognitiveLoadScore");
    }

    [Fact]
    public void Constructor_WithNegativeOverloadEventCount_ThrowsArgumentOutOfRangeException()
    {
        var act = () => Build(overloadCount: -1);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("overloadEventCount");
    }

    [Fact]
    public void Constructor_WithNegativeReadingCount_ThrowsArgumentOutOfRangeException()
    {
        var act = () => Build(readingCount: -1);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("readingCount");
    }
}
