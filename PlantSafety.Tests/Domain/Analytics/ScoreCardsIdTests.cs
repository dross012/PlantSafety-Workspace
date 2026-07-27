// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.ScoreCards
//  Layer:          Tests (Unit)
//  File:           ScoreCardsIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ScoreCardsId StrongId value object.
// ============================================================================
using PlantSafety.Domain.Analytics.ScoreCards.ValueObjects.Ids;

namespace PlantSafety.Tests.Analytics.ScoreCards;

public sealed class ScoreCardsIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = ScoreCardsId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => ScoreCardsId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        ScoreCardsId.New().Should().NotBe(ScoreCardsId.New());
}
