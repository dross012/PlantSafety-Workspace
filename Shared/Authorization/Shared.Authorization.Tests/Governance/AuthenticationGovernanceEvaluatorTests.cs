// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           AuthenticationGovernanceEvaluatorTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Identity;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Infrastructure.Governance;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Authorization.Tests.Governance;

public sealed class AuthenticationGovernanceEvaluatorTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void EvaluateAndReport_ValidResolution_WritesNoLogEvent()
    {
        var writer = new CapturingLogEventWriter();
        var caller = CallerContext.SystemDefault("corr-1");
        var resolution = new CallerContextResolution(caller, ApiKeyIdentity.NoKeyProvided(EnterpriseRole.PlatformAdmin));

        AuthenticationGovernanceEvaluator.EvaluateAndReport(resolution, Clock, writer).Should().BeEmpty();
        writer.LastEvent.Should().BeNull();
    }

    [Fact]
    public void EvaluateAndReport_InvalidResolution_WritesGovernanceViolationLogEvent()
    {
        var writer = new CapturingLogEventWriter();

        IReadOnlyList<string> violations = AuthenticationGovernanceEvaluator.EvaluateAndReport(null, Clock, writer);

        violations.Should().NotBeEmpty();
        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("AUTHENTICATION_GOVERNANCE_VIOLATION");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Warning);
        writer.LastEvent.Category.Should().Be(LogCategory.Authentication);
    }
}
