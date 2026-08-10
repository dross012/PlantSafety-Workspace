// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingTenantTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Scheduling.Abstractions.Rules;
using Shared.Scheduling.Abstractions.Schedule;
using Shared.Scheduling.Infrastructure.Authorization;
using Shared.Scheduling.Infrastructure.Tenant;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Actions;

using Xunit;

namespace Shared.Scheduling.Tests.Tenant;

public sealed class SchedulingTenantTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public void Write(ILogEvent logEvent)
        {
        }
    }

    private sealed class FakeResolver(bool canAccess) : ITenantFacilityAccessResolver
    {
        public bool CanAccessFacility(string tenantId, Guid facilityId) => canAccess;
    }

    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly ILogEventWriter Writer = new CapturingLogEventWriter();
    private static readonly CallerContext Caller = new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

    private static readonly SchedulingEvaluationContext Context = new(
        new[] { new RequiredAction("ACT-1", "Action", DueWithinHours: 6) }, OverdueActionCount: 0, ActiveMachineGroupCount: 0);

    [Fact]
    public async Task CalculateWithTenantEnforcementAsync_UnscopedTenant_MatchesDirectAuthorizationResult()
    {
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        SchedulingSchedule viaWrapper = await SchedulingTenant.CalculateWithTenantEnforcementAsync(
            Context, Clock.UtcNow, Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, Writer);
        SchedulingSchedule direct = await SchedulingAuthorization.CalculateWithAuthorizationAsync(Context, Clock.UtcNow, Caller, Resolver, Clock, Writer);

        viaWrapper.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task CalculateWithTenantEnforcementAsync_NullRequestedFacility_AllowsRegardlessOfTenantScope()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: Guid.NewGuid(), Caller, "corr-2");

        Func<Task> act = () => SchedulingTenant.CalculateWithTenantEnforcementAsync(
            Context, Clock.UtcNow, Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CalculateWithTenantEnforcementAsync_CrossFacilityRequest_ThrowsUnauthorizedAccessException()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: Guid.NewGuid(), Caller, "corr-3");

        Func<Task> act = () => SchedulingTenant.CalculateWithTenantEnforcementAsync(
            Context, Clock.UtcNow, Caller, tenantContext, requestedFacilityId: Guid.NewGuid(), Resolver, tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task CalculateWithTenantEnforcementAsync_ResolverGrantsAccess_Succeeds()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-4");

        Func<Task> act = () => SchedulingTenant.CalculateWithTenantEnforcementAsync(
            Context, Clock.UtcNow, Caller, tenantContext, requestedFacilityId: Guid.NewGuid(), Resolver, new FakeResolver(true), Clock, Writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CalculateWithTenantEnforcementAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var deniedCaller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-5");
        TenantContext tenantContext = TenantContext.Unscoped(deniedCaller, "corr-5");

        Func<Task> act = () => SchedulingTenant.CalculateWithTenantEnforcementAsync(
            Context, Clock.UtcNow, deniedCaller, tenantContext, requestedFacilityId: null, new AlwaysDenyPermissionResolver(), tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
