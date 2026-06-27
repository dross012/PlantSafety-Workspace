// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.PredictiveAnalytics
//  Layer:          Tests (Unit)
//  File:           PredictiveAnalyticsIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for PredictiveAnalyticsId StrongId value object.
// ============================================================================
namespace PlantSafety.Tests.Analytics.PredictiveAnalytics;

public sealed class PredictiveAnalyticsIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = PredictiveAnalyticsId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => PredictiveAnalyticsId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        PredictiveAnalyticsId.New().Should().NotBe(PredictiveAnalyticsId.New());
}
