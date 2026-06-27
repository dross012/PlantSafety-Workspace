// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Common.TestBase
//  Layer:          Tests (Unit)
//  File:           FakeLoggerTests.cs
//  Created:        2026-06-17
//  SchemaVersion:  2026.1
//  Description:    Unit tests verifying FakeLogger<T> captures log entries correctly.
// ============================================================================
namespace PlantSafety.Tests.Common.TestBase;

public sealed class FakeLoggerTests : UnitTestBase
{
    [Fact]
    public void Log_ShouldCaptureEntry()
    {
        var logger = new FakeLogger<FakeLoggerTests>();
        logger.LogInformation("test message");
        logger.Entries.Should().HaveCount(1);
    }

    [Fact]
    public void Log_ShouldCaptureLevel()
    {
        var logger = new FakeLogger<FakeLoggerTests>();
        logger.LogWarning("warn");
        logger.Entries[0].Level.Should().Be(LogLevel.Warning);
    }

    [Fact]
    public void Log_ShouldCaptureMessage()
    {
        var logger = new FakeLogger<FakeLoggerTests>();
        logger.LogError("error msg");
        logger.Entries[0].Message.Should().Contain("error msg");
    }

    [Fact]
    public void ShouldHaveLogged_WithMatchingLevel_ShouldNotThrow()
    {
        var logger = new FakeLogger<FakeLoggerTests>();
        logger.LogError("oops");
        var act = () => logger.ShouldHaveLogged(LogLevel.Error);
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveNoErrors_WithInfoOnly_ShouldNotThrow()
    {
        var logger = new FakeLogger<FakeLoggerTests>();
        logger.LogInformation("ok");
        var act = () => logger.ShouldHaveNoErrors();
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveNoErrors_WithErrorLogged_ShouldThrow()
    {
        var logger = new FakeLogger<FakeLoggerTests>();
        logger.LogError("fail");
        var act = () => logger.ShouldHaveNoErrors();
        act.Should().Throw<Exception>();
    }
}
