// ============================================================================
//  Project:        Shared.Telemetry.Tests
//  File:           TelemetryGovernanceEvaluatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Governance;

using Xunit;

namespace Shared.Telemetry.Tests.Governance;

public sealed class TelemetryGovernanceEvaluatorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    [Fact]
    public void EvaluateAndReport_ValidEvent_ReturnsNoViolationsAndWritesNothing()
    {
        var writer = new CapturingLogEventWriter();
        var evt = new LogEvent("VALID_EVENT", LogCategory.Infrastructure, LogSeverity.Info, "ok",
            new Dictionary<string, object?>(), "corr-1", "Test", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        IReadOnlyList<string> violations = TelemetryGovernanceEvaluator.EvaluateAndReport(evt, writer);

        violations.Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void EvaluateAndReport_InvalidEvent_ReturnsViolationsAndWritesGovernanceViolationEvent()
    {
        var writer = new CapturingLogEventWriter();
        var evt = new LogEvent("invalid_event", LogCategory.Infrastructure, LogSeverity.Info, "ok",
            new Dictionary<string, object?>(), "", "Test", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        IReadOnlyList<string> violations = TelemetryGovernanceEvaluator.EvaluateAndReport(evt, writer);

        violations.Should().NotBeEmpty();
        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("TELEMETRY_GOVERNANCE_VIOLATION");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Warning);
        writer.LastEvent.Module.Should().Be("Governance");
    }
}
