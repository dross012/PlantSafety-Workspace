// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Engagement
//  Layer:          Tests (Unit)
//  File:           EngagementIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for EngagementId StrongId value object.
// ============================================================================
namespace PlantSafety.Tests.Analytics.Engagement;

public sealed class EngagementIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = EngagementId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => EngagementId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        EngagementId.New().Should().NotBe(EngagementId.New());
}
