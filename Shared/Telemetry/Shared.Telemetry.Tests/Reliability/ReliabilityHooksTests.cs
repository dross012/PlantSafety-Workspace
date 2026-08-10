// ============================================================================
//  Project:        Shared.Telemetry.Tests
//  File:           ReliabilityHooksTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Reliability;

using Xunit;

namespace Shared.Telemetry.Tests.Reliability;

public sealed class ReliabilityHooksTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    [Fact]
    public async Task ExecuteAsync_OperationSucceeds_ReturnsResultAndLogsSuccess()
    {
        var clock = new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var writer = new CapturingLogEventWriter();

        int result = await ReliabilityHooks.ExecuteAsync("TestModule", "TestOp", clock, writer, "corr-1", () => Task.FromResult(42));

        result.Should().Be(42);
        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("RELIABILITY_OPERATION_SUCCEEDED");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Info);
        writer.LastEvent.CorrelationId.Should().Be("corr-1");
        writer.LastEvent.Module.Should().Be("TestModule");
    }

    [Fact]
    public async Task ExecuteAsync_OperationThrows_RethrowsAndLogsFailure()
    {
        var clock = new FixedClock(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var writer = new CapturingLogEventWriter();

        Func<Task> act = () => ReliabilityHooks.ExecuteAsync<int>(
            "TestModule", "TestOp", clock, writer, "corr-2", () => throw new InvalidOperationException("boom"));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("RELIABILITY_OPERATION_FAILED");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Error);
        writer.LastEvent.Payload["ExceptionType"].Should().Be(nameof(InvalidOperationException));
    }

    [Fact]
    public async Task ExecuteAsync_RecordsDurationFromClockNotWallClock()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var clock = new FixedClock(start);
        var writer = new CapturingLogEventWriter();

        await ReliabilityHooks.ExecuteAsync("TestModule", "TestOp", clock, writer, "corr-3", () => Task.FromResult(1));

        // FixedClock never advances, so duration must be deterministically zero.
        writer.LastEvent!.Payload["DurationMs"].Should().Be(0d);
    }
}
