// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Analytics.Culture
//  Layer:          Tests (Unit)
//  File:           CultureIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for CultureId StrongId value object.
// ============================================================================
using PlantSafety.Domain.Analytics.Culture.ValueObjects.Ids;

namespace PlantSafety.Tests.Analytics.Culture;

public sealed class CultureIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = CultureId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => CultureId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_DifferentInstances_ShouldNotBeEqual() =>
        CultureId.New().Should().NotBe(CultureId.New());
}
