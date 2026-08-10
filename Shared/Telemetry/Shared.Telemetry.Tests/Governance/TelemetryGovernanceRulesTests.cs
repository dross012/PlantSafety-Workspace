// ============================================================================
//  Project:        Shared.Telemetry.Tests
//  File:           TelemetryGovernanceRulesTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Telemetry.Abstractions.Events;
using Shared.Telemetry.Abstractions.Governance;

using Xunit;

namespace Shared.Telemetry.Tests.Governance;

public sealed class TelemetryGovernanceRulesTests
{
    private static LogEvent ValidEvent() => new(
        EventId: "TEST_EVENT_OCCURRED",
        Category: LogCategory.Infrastructure,
        Severity: LogSeverity.Info,
        Message: "A test event occurred.",
        Payload: new Dictionary<string, object?>(),
        CorrelationId: "corr-1",
        Module: "Test",
        OccurredAtUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void ValidateLogEvent_ValidEvent_ReturnsNoViolations()
    {
        TelemetryGovernanceRules.ValidateLogEvent(ValidEvent()).Should().BeEmpty();
    }

    [Fact]
    public void ValidateLogEvent_LowercaseEventId_ReturnsViolation()
    {
        LogEvent evt = ValidEvent() with { EventId = "test_event" };

        TelemetryGovernanceRules.ValidateLogEvent(evt).Should().Contain(v => v.Contains("EventId"));
    }

    [Fact]
    public void ValidateLogEvent_EmptyCorrelationId_ReturnsViolation()
    {
        LogEvent evt = ValidEvent() with { CorrelationId = "" };

        TelemetryGovernanceRules.ValidateLogEvent(evt).Should().Contain(v => v.Contains("CorrelationId"));
    }

    [Fact]
    public void ValidateLogEvent_EmptyModule_ReturnsViolation()
    {
        LogEvent evt = ValidEvent() with { Module = "" };

        TelemetryGovernanceRules.ValidateLogEvent(evt).Should().Contain(v => v.Contains("Module"));
    }

    [Fact]
    public void ValidateLogEvent_DefaultOccurredAtUtc_ReturnsViolation()
    {
        LogEvent evt = ValidEvent() with { OccurredAtUtc = default };

        TelemetryGovernanceRules.ValidateLogEvent(evt).Should().Contain(v => v.Contains("OccurredAtUtc"));
    }

    [Theory]
    [InlineData("plantsafety.requests_handled", true)]
    [InlineData("plantsafety.workflow_decisions_evaluated", true)]
    [InlineData("PlantSafety.RequestsHandled", false)]
    [InlineData("requests_handled", false)]
    [InlineData("", false)]
    public void ValidateMetricName_MatchesConvention(string metricName, bool expectedValid)
    {
        IReadOnlyList<string> violations = TelemetryGovernanceRules.ValidateMetricName(metricName);

        (violations.Count == 0).Should().Be(expectedValid);
    }

    [Fact]
    public void ValidateCorrelationPropagation_NullOrWhitespace_ReturnsViolation()
    {
        TelemetryGovernanceRules.ValidateCorrelationPropagation(null).Should().NotBeEmpty();
        TelemetryGovernanceRules.ValidateCorrelationPropagation("   ").Should().NotBeEmpty();
        TelemetryGovernanceRules.ValidateCorrelationPropagation("corr-1").Should().BeEmpty();
    }
}
