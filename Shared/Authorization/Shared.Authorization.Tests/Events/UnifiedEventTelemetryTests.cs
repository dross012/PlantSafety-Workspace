// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           UnifiedEventTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;
using Shared.Authorization.Infrastructure.Events;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Authorization.Tests.Events;

public sealed class UnifiedEventTelemetryTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly ContractMetadata Metadata = new("Safety.Editor", "corr-1", MfaStatus.NotApplicable, "acme-plant-1", Clock.UtcNow);

    [Fact]
    public void ValidateAndReport_ConsistentEvent_WritesNoLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var logEvent = new LogEvent("TEST_EVENT", LogCategory.Infrastructure, LogSeverity.Info, "test", new Dictionary<string, object?>(), "corr-1", "Test", Clock.UtcNow);

        UnifiedEventTelemetry.ValidateAndReport(logEvent, Metadata, Clock, writer).Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void ValidateAndReport_InconsistentEvent_WritesGovernanceViolationLogEvent()
    {
        var writer = new CapturingLogEventWriter();

        IReadOnlyList<string> violations = UnifiedEventTelemetry.ValidateAndReport(null, Metadata, Clock, writer);

        violations.Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("EVENT_GOVERNANCE_VIOLATION");
    }
}
