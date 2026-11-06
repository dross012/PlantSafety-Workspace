// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           AuthorizationTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Checks;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Authorization.Tests.Telemetry;

public sealed class AuthorizationTelemetryTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void RecordDecision_Allowed_WritesInfoLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");
        AuthorizationDecision decision = AuthorizationDecision.Allow("granted");

        AuthorizationTelemetry.RecordDecision(decision, caller, "TestModule", "Read", Clock, writer);

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("AUTHORIZATION_CHECK_PERFORMED");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Info);
        writer.LastEvent.CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public void RecordDecision_Denied_WritesWarningLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-2");
        AuthorizationDecision decision = AuthorizationDecision.Deny("not granted");

        AuthorizationTelemetry.RecordDecision(decision, caller, "TestModule", "Admin", Clock, writer);

        writer.LastEvent!.Severity.Should().Be(LogSeverity.Warning);
    }

    [Fact]
    public void RecordMfaFailure_WritesMfaRequirementNotMetLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotVerified, "corr-3");

        AuthorizationTelemetry.RecordMfaFailure(caller, "TestModule", Clock, writer);

        writer.LastEvent!.EventId.Should().Be("MFA_REQUIREMENT_NOT_MET");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Warning);
    }
}
