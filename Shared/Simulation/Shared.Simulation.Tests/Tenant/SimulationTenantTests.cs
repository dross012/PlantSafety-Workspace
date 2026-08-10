// ============================================================================
//  Project:        Shared.Simulation.Tests
//  File:           SimulationTenantTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.Simulation.Abstractions.Simulation;
using Shared.Simulation.Infrastructure.Authorization;
using Shared.Simulation.Infrastructure.Tenant;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.Simulation.Tests.Tenant;

public sealed class SimulationTenantTests
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

    private static SimulationEvaluationRequestContract BuildRequest() => new(
        new SimulationMetadata(SimulationMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow),
        new SimulationScenario("scenario-1", "Test", new[] { new SimulationStep(0, SimulationStepType.Hazard, "x") }),
        null, null);

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_UnscopedTenant_MatchesDirectAuthorizationResult()
    {
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");
        SimulationEvaluationRequestContract request = BuildRequest();

        var viaWrapper = await SimulationTenant.EvaluateWithTenantEnforcementAsync(
            request, Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, Writer);
        var direct = await SimulationAuthorization.EvaluateWithAuthorizationAsync(request, Caller, Resolver, Clock, Writer);

        viaWrapper.Outcome.OverallPriority.Should().Be(direct.Outcome.OverallPriority);
    }

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_CrossFacilityRequest_ThrowsUnauthorizedAccessException()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: Guid.NewGuid(), Caller, "corr-3");

        Func<Task> act = () => SimulationTenant.EvaluateWithTenantEnforcementAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: Guid.NewGuid(), Resolver, tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_ResolverGrantsAccess_Succeeds()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-4");

        Func<Task> act = () => SimulationTenant.EvaluateWithTenantEnforcementAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: Guid.NewGuid(), Resolver, new FakeResolver(true), Clock, Writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var deniedCaller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-5");
        TenantContext tenantContext = TenantContext.Unscoped(deniedCaller, "corr-5");

        Func<Task> act = () => SimulationTenant.EvaluateWithTenantEnforcementAsync(
            BuildRequest(), deniedCaller, tenantContext, requestedFacilityId: null, new AlwaysDenyPermissionResolver(), tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
