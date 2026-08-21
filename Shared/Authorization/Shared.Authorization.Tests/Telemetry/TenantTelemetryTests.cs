// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           TenantTelemetryTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Telemetry;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Authorization.Tests.Telemetry;

public sealed class TenantTelemetryTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly CallerContext Caller = new(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-1");

    [Fact]
    public void RecordResolution_NoKeyProvided_WritesInfoLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var context = TenantContext.Unscoped(Caller, "corr-1");
        var resolution = new TenantContextResolution(context, TenantKeyProvided: false, TenantRecognized: false);

        TenantTelemetry.RecordResolution(resolution, Clock, writer);

        writer.LastEvent!.EventId.Should().Be("TENANT_RESOLVED");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Info);
        writer.LastEvent.Category.Should().Be(LogCategory.Tenant);
        writer.LastEvent.CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public void RecordResolution_UnrecognizedKey_WritesWarningLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var context = TenantContext.Unscoped(Caller, "corr-2");
        var resolution = new TenantContextResolution(context, TenantKeyProvided: true, TenantRecognized: false);

        TenantTelemetry.RecordResolution(resolution, Clock, writer);

        writer.LastEvent!.Severity.Should().Be(LogSeverity.Warning);
    }

    [Fact]
    public void RecordIsolationDecision_Allowed_WritesInfoLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var context = new TenantContext("acme-plant-1", null, Caller, "corr-3");
        TenantAccessDecision decision = TenantAccessDecision.Allow("allowed");

        TenantTelemetry.RecordIsolationDecision(decision, context, "WorkflowEngine", Clock, writer);

        writer.LastEvent!.EventId.Should().Be("TENANT_ISOLATION_CHECK_PERFORMED");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Info);
    }

    [Fact]
    public void RecordIsolationDecision_Denied_WritesWarningLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var context = new TenantContext("acme-plant-1", null, Caller, "corr-4");
        TenantAccessDecision decision = TenantAccessDecision.Deny("denied");

        TenantTelemetry.RecordIsolationDecision(decision, context, "WorkflowEngine", Clock, writer);

        writer.LastEvent!.Severity.Should().Be(LogSeverity.Warning);
    }
}
