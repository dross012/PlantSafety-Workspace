// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.TestBase
//  Layer:          Tests (Unit)
//  File:           FakeClockTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests verifying FakeClock determinism and AdvanceBy behavior.
// ============================================================================
namespace PlantSafety.Tests.Common.TestBase;

public sealed class FakeClockTests : UnitTestBase
{
    [Fact]
    public void UtcNow_ShouldReturnFixedDate()
    {
        var clock = new FakeClock();
        clock.UtcNow.Should().Be(FixedUtcNow);
    }

    [Fact]
    public void AdvanceBy_ShouldMoveClockForward()
    {
        var clock = new FakeClock();
        clock.AdvanceBy(TimeSpan.FromMinutes(5));
        clock.UtcNow.Should().Be(FixedUtcNow.AddMinutes(5));
    }

    [Fact]
    public void Reset_ShouldReturnClockToInitialValue()
    {
        var clock = new FakeClock();
        clock.AdvanceBy(TimeSpan.FromHours(1));
        clock.Reset();
        clock.UtcNow.Should().Be(FixedUtcNow);
    }

    [Fact]
    public void Now_ShouldReturnUtcTimestampWrappingCurrentValue()
    {
        var clock = new FakeClock();
        clock.Now().Should().Be(FixedTimestamp);
    }
}
