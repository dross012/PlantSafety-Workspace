// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           TenantGovernanceEvaluatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Authorization.Tests.Governance;

public sealed class TenantGovernanceEvaluatorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly CallerContext Caller = new(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-1");

    [Fact]
    public void EvaluateResolutionAndReport_Valid_WritesNoLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var context = TenantContext.Unscoped(Caller, "corr-1");
        var resolution = new TenantContextResolution(context, TenantKeyProvided: false, TenantRecognized: false);

        TenantGovernanceEvaluator.EvaluateResolutionAndReport(resolution, Clock, writer).Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void EvaluateResolutionAndReport_Invalid_WritesGovernanceViolationLogEvent()
    {
        var writer = new CapturingLogEventWriter();

        IReadOnlyList<string> violations = TenantGovernanceEvaluator.EvaluateResolutionAndReport(null, Clock, writer);

        violations.Should().NotBeEmpty();
        writer.LastEvent!.EventId.Should().Be("TENANT_GOVERNANCE_VIOLATION");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Warning);
        writer.LastEvent.Category.Should().Be(LogCategory.Tenant);
    }

    [Fact]
    public void EvaluateContextAndReport_Valid_WritesNoLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var context = TenantContext.Unscoped(Caller, "corr-1");

        TenantGovernanceEvaluator.EvaluateContextAndReport(context, Clock, writer).Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void EvaluateIsolationDecisionAndReport_DenialWithoutFacility_WritesGovernanceViolationLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        TenantAccessDecision decision = TenantAccessDecision.Deny("denied");

        IReadOnlyList<string> violations = TenantGovernanceEvaluator.EvaluateIsolationDecisionAndReport(decision, null, "corr-1", Clock, writer);

        violations.Should().Contain("DenialWithoutRequestedFacility");
        writer.LastEvent!.EventId.Should().Be("TENANT_GOVERNANCE_VIOLATION");
    }
}
