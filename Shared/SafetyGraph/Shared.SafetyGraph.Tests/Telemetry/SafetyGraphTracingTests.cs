// ============================================================================
//  Project:        Shared.SafetyGraph.Tests
//  File:           SafetyGraphTracingTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.SafetyGraph.Infrastructure.Telemetry;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SafetyGraph.Tests.Telemetry;

public sealed class SafetyGraphTracingTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    [Fact]
    public void RecordSpan_WritesTraceSpanLogEventWithComputedDuration()
    {
        var writer = new CapturingLogEventWriter();
        var startedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endedAtUtc = startedAtUtc.AddMilliseconds(400);

        SafetyGraphTracing.RecordSpan("TestSpan", startedAtUtc, endedAtUtc, "corr-1", writer);

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("SAFETY_GRAPH_TRACE_SPAN");
        writer.LastEvent.Payload["DurationMs"].Should().Be(400.0);
    }

    [Fact]
    public void RecordSpan_NullSpanName_Throws()
    {
        var writer = new CapturingLogEventWriter();
        var now = DateTime.UtcNow;

        Action act = () => SafetyGraphTracing.RecordSpan(null!, now, now, "corr-1", writer);

        act.Should().Throw<ArgumentNullException>();
    }
}
