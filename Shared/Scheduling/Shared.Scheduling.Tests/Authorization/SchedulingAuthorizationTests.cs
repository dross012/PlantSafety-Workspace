// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingAuthorizationTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.Scheduling.Infrastructure.Authorization;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Actions;

using Xunit;

namespace Shared.Scheduling.Tests.Authorization;

public sealed class SchedulingAuthorizationTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public ILogEvent? LastEvent { get; private set; }
        public void Write(ILogEvent logEvent) => LastEvent = logEvent;
    }

    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly SchedulingEvaluationContext Context = new(
        new[] { new RequiredAction("ACT-1", "Action", DueWithinHours: 6) }, OverdueActionCount: 0, ActiveMachineGroupCount: 0);

    [Fact]
    public async Task CalculateWithAuthorizationAsync_PlatformAdmin_ReturnsSameScheduleAsUnwrappedCalculate()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

        SchedulingSchedule result = await SchedulingAuthorization.CalculateWithAuthorizationAsync(Context, Clock.UtcNow, caller, Resolver, Clock, writer);
        SchedulingSchedule direct = SchedulingScheduleCalculator.Calculate(Context, Clock.UtcNow);

        result.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task CalculateWithAuthorizationAsync_ViewerRole_IsAlsoAuthorized()
    {
        // Read is the lowest tier; even Viewer should be authorized for a read-only evaluation.
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-2");

        Func<Task> act = () => SchedulingAuthorization.CalculateWithAuthorizationAsync(Context, Clock.UtcNow, caller, Resolver, Clock, writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CalculateWithAuthorizationAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-3");
        var denyingResolver = new AlwaysDenyPermissionResolver();

        Func<Task> act = () => SchedulingAuthorization.CalculateWithAuthorizationAsync(Context, Clock.UtcNow, caller, denyingResolver, Clock, writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task CalculateWithAuthorizationAsync_Denied_EmitsAuthorizationCheckPerformedLogEventBeforeThrowing()
    {
        var writer = new CapturingLogEventWriter();
        var caller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-4");
        var denyingResolver = new AlwaysDenyPermissionResolver();

        try
        {
            await SchedulingAuthorization.CalculateWithAuthorizationAsync(Context, Clock.UtcNow, caller, denyingResolver, Clock, writer);
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
