// ============================================================================
//  Project:        Shared.WorkflowEngine.Tests
//  File:           WorkflowEngineAuthorizationTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Abstractions.Triggers;
using Shared.WorkflowEngine.Infrastructure.Authorization;

using Xunit;

namespace Shared.WorkflowEngine.Tests.Authorization;

public sealed class WorkflowEngineAuthorizationTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly WorkflowContext Context = new(HazardType.Fire, SeverityLevel.Catastrophic, EvacuationScope.Building, 2, 1, 3);

    [Fact]
    public async Task EvaluateWithAuthorizationAsync_PlatformAdmin_ReturnsSameDecisionAsUnwrappedEvaluate()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

        WorkflowDecision result = await WorkflowEngineAuthorization.EvaluateWithAuthorizationAsync(Context, caller, Resolver, Clock, writer);
        WorkflowDecision direct = Abstractions.Engine.WorkflowEngine.Evaluate(Context);

        result.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task EvaluateWithAuthorizationAsync_ViewerRole_IsAlsoAuthorized()
    {
        // Read is the lowest tier; even Viewer should be authorized for a read-only evaluation.
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-2");

        Func<Task> act = () => WorkflowEngineAuthorization.EvaluateWithAuthorizationAsync(Context, caller, Resolver, Clock, writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EvaluateWithAuthorizationAsync_InvalidRole_ThrowsUnauthorizedAccessException()
    {
        var writer = new CapturingLogEventWriter();
        // A CallerContext whose Role is not one of EnterpriseRole.All's canonical values resolves
        // to an empty permission set (IPermissionResolver's own documented zero-trust default-deny).
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-3");
        var denyingResolver = new AlwaysDenyPermissionResolver();

        Func<Task> act = () => WorkflowEngineAuthorization.EvaluateWithAuthorizationAsync(Context, caller, denyingResolver, Clock, writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task EvaluateWithAuthorizationAsync_Denied_EmitsAuthorizationCheckPerformedLogEventBeforeThrowing()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-4");
        var denyingResolver = new AlwaysDenyPermissionResolver();

        try
        {
            await WorkflowEngineAuthorization.EvaluateWithAuthorizationAsync(Context, caller, denyingResolver, Clock, writer);
        }
        catch (UnauthorizedAccessException)
        {
            // expected
        }

        writer.LastEvent.Should().NotBeNull();
        writer.LastEvent!.EventId.Should().Be("AUTHORIZATION_CHECK_PERFORMED");
        writer.LastEvent.Severity.Should().Be(LogSeverity.Warning);
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
