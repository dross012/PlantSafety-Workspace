// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           FacilityTenantTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Infrastructure.Tenant;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.FacilityDomain.Tests.Tenant;

public sealed class FacilityTenantTests
{
    private sealed class CapturingLogEventWriter : ILogEventWriter
    {
        public void Write(ILogEvent logEvent)
        {
        }
    }

    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly ILogEventWriter Writer = new CapturingLogEventWriter();
    private static readonly CallerContext Caller = new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

    [Fact]
    public async Task InferHazardTypeWithTenantEnforcementAsync_UnscopedTenant_MatchesDirectRuleResult()
    {
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        HazardType viaWrapper = await FacilityTenant.InferHazardTypeWithTenantEnforcementAsync(
            "Fire in warehouse", null, Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, Writer);
        HazardType direct = HazardInferenceRules.InferHazardTypeFromSafetyEvent("Fire in warehouse", null);

        viaWrapper.Should().Be(direct);
    }

    [Fact]
    public async Task InferHazardTypeWithTenantEnforcementAsync_CrossFacilityRequest_ThrowsUnauthorizedAccessException()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: Guid.NewGuid(), Caller, "corr-2");

        Func<Task> act = () => FacilityTenant.InferHazardTypeWithTenantEnforcementAsync(
            "Fire", null, Caller, tenantContext, requestedFacilityId: Guid.NewGuid(), Resolver, tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task InferHazardTypeWithTenantEnforcementAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var deniedCaller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-3");
        TenantContext tenantContext = TenantContext.Unscoped(deniedCaller, "corr-3");

        Func<Task> act = () => FacilityTenant.InferHazardTypeWithTenantEnforcementAsync(
            "Fire", null, deniedCaller, tenantContext, requestedFacilityId: null, new AlwaysDenyPermissionResolver(), tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
