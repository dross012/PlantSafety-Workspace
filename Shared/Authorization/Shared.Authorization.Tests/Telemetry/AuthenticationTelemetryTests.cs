// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           AuthenticationTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Authorization.Tests.Telemetry;

public sealed class AuthenticationTelemetryTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private sealed class FixedCorrelationContext : ICorrelationContext
    {
        public string CorrelationId => "test-correlation";
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void RecordResolution_NoKeyProvided_WritesInfoLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var caller = CallerContext.SystemDefault("corr-1");
        var resolution = new CallerContextResolution(caller, ApiKeyIdentity.NoKeyProvided(EnterpriseRole.PlatformAdmin));

        AuthenticationTelemetry.RecordResolution(resolution, Clock, writer);

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("API_KEY_RESOLVED");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Info);
        writer.LastEvent.Category.Should().Be(LogCategory.Authentication);
        writer.LastEvent.CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public void RecordResolution_UnrecognizedKey_WritesWarningLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var caller = CallerContext.SystemDefault("corr-2");
        var resolution = new CallerContextResolution(caller, ApiKeyIdentity.UnrecognizedKey(EnterpriseRole.PlatformAdmin));

        AuthenticationTelemetry.RecordResolution(resolution, Clock, writer);

        writer.LastEvent!.Severity.Should().Be(LogSeverity.Warning);
    }

    [Fact]
    public void RecordResolution_RecognizedKey_WritesInfoLogEventWithResolvedRole()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-3");
        var resolution = new CallerContextResolution(caller, ApiKeyIdentity.Recognized(EnterpriseRole.Editor));

        AuthenticationTelemetry.RecordResolution(resolution, Clock, writer);

        writer.LastEvent!.Severity.Should().Be(LogSeverity.Info);
        writer.LastEvent.Payload!["ResolvedRole"].Should().Be("Safety.Editor");
    }
}
