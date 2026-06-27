// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Compliance.RegulatoryCalendar
//  Layer:          Tests (Unit)
//  File:           RegulatoryCalendarIdTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests for RegulatoryCalendarId StrongId value object.
// ============================================================================
namespace PlantSafety.Tests.Compliance.RegulatoryCalendar;

public sealed class RegulatoryCalendarIdTests : UnitTestBase
{
    [Fact]
    public void New_ShouldReturnNonEmptyId()
    {
        var id = RegulatoryCalendarId.New();
        id.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEmptyGuid_ShouldThrow() =>
        FluentActions.Invoking(() => RegulatoryCalendarId.FromGuid(Guid.Empty))
                     .Should().Throw<ArgumentException>();

    [Fact]
    public void TwoIds_WithSameGuid_ShouldBeEqual()
    {
        var g = Guid.NewGuid();
        RegulatoryCalendarId.FromGuid(g).Should().Be(RegulatoryCalendarId.FromGuid(g));
    }

    [Fact]
    public void TwoIds_WithDifferentGuids_ShouldNotBeEqual() =>
        RegulatoryCalendarId.New().Should().NotBe(RegulatoryCalendarId.New());
}
