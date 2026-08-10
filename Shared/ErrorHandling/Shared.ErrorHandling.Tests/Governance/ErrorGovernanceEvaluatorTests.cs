// ============================================================================
//  Project:        Shared.ErrorHandling.Tests
//  File:           ErrorGovernanceEvaluatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.ErrorHandling.Abstractions.Envelope;
using Shared.ErrorHandling.Infrastructure.Governance;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.ErrorHandling.Tests.Governance;

public sealed class ErrorGovernanceEvaluatorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void EvaluateAndReport_ValidEnvelope_WritesNoLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var envelope = new ErrorEnvelope("WORKFLOW-VALIDATION", ErrorCategory.Validation, "user msg", "dev msg", "corr-1", "WorkflowEngine", Clock.UtcNow);

        ErrorGovernanceEvaluator.EvaluateAndReport(envelope, Clock, writer).Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void EvaluateAndReport_InvalidEnvelope_WritesGovernanceViolationLogEvent()
    {
        var writer = new CapturingLogEventWriter();

        IReadOnlyList<string> violations = ErrorGovernanceEvaluator.EvaluateAndReport(null, Clock, writer);

        violations.Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("ERROR_GOVERNANCE_VIOLATION");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Warning);
    }
}
