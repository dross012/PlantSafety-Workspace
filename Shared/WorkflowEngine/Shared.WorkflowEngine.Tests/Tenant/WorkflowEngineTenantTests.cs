// ============================================================================
//  Project:        Shared.WorkflowEngine.Tests
//  File:           WorkflowEngineTenantTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.Telemetry.Abstractions.Events;
using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Abstractions.Triggers;
using Shared.WorkflowEngine.Infrastructure.Authorization;
using Shared.WorkflowEngine.Infrastructure.Tenant;

using Xunit;

namespace Shared.WorkflowEngine.Tests.Tenant;

public sealed class WorkflowEngineTenantTests
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

    private static readonly WorkflowContext Context = new(
        HazardType.Fire, SeverityLevel.Severe, EvacuationScope.Zone, EmployeeExposureCount: 2, MachineFailureCount: 0, EventChainLength: 1);

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_UnscopedTenant_MatchesDirectAuthorizationResult()
    {
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        WorkflowDecision viaWrapper = await WorkflowEngineTenant.EvaluateWithTenantEnforcementAsync(
            Context, Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, Writer);
        WorkflowDecision direct = await WorkflowEngineAuthorization.EvaluateWithAuthorizationAsync(Context, Caller, Resolver, Clock, Writer);

        viaWrapper.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_NullRequestedFacility_AllowsRegardlessOfTenantScope()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: Guid.NewGuid(), Caller, "corr-2");

        Func<Task> act = () => WorkflowEngineTenant.EvaluateWithTenantEnforcementAsync(
            Context, Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_CrossFacilityRequest_ThrowsUnauthorizedAccessException()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: Guid.NewGuid(), Caller, "corr-3");

        Func<Task> act = () => WorkflowEngineTenant.EvaluateWithTenantEnforcementAsync(
            Context, Caller, tenantContext, requestedFacilityId: Guid.NewGuid(), Resolver, tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_ResolverGrantsAccess_Succeeds()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-4");

        Func<Task> act = () => WorkflowEngineTenant.EvaluateWithTenantEnforcementAsync(
            Context, Caller, tenantContext, requestedFacilityId: Guid.NewGuid(), Resolver, new FakeResolver(true), Clock, Writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var deniedCaller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-5");
        TenantContext tenantContext = TenantContext.Unscoped(deniedCaller, "corr-5");

        Func<Task> act = () => WorkflowEngineTenant.EvaluateWithTenantEnforcementAsync(
            Context, deniedCaller, tenantContext, requestedFacilityId: null, new AlwaysDenyPermissionResolver(), tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
