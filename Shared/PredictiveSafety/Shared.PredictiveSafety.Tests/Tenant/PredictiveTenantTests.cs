// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveTenantTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Infrastructure.Authorization;
using Shared.PredictiveSafety.Infrastructure.Tenant;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Tenant;

public sealed class PredictiveTenantTests
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

    private static PredictiveSafetyEvaluationRequestContract BuildRequest() => new(
        new PredictiveSafetyContractMetadata(PredictiveSafetyContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", Clock.UtcNow),
        "ChemicalSpill", null, 0, 0, 0, null, Guid.NewGuid(), null, 0, null, 0, 0, 0, 0, 0, Clock.UtcNow);

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_UnscopedTenant_MatchesDirectAuthorizationResult()
    {
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");
        PredictiveSafetyEvaluationRequestContract request = BuildRequest();

        var viaWrapper = await PredictiveTenant.EvaluateWithTenantEnforcementAsync(
            request, Caller, tenantContext, requestedFacilityId: null, Resolver, tenantFacilityAccessResolver: null, Clock, Writer);
        var direct = await PredictiveAuthorization.EvaluateWithAuthorizationAsync(request, Caller, Resolver, Clock, Writer);

        viaWrapper.OverallPriority.Should().Be(direct.OverallPriority);
    }

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_CrossFacilityRequest_ThrowsUnauthorizedAccessException()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: Guid.NewGuid(), Caller, "corr-3");

        Func<Task> act = () => PredictiveTenant.EvaluateWithTenantEnforcementAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: Guid.NewGuid(), Resolver, tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_ResolverGrantsAccess_Succeeds()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-4");

        Func<Task> act = () => PredictiveTenant.EvaluateWithTenantEnforcementAsync(
            BuildRequest(), Caller, tenantContext, requestedFacilityId: Guid.NewGuid(), Resolver, new FakeResolver(true), Clock, Writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EvaluateWithTenantEnforcementAsync_DeniedCaller_ThrowsUnauthorizedAccessException()
    {
        var deniedCaller = new CallerContext(EnterpriseRole.Viewer, MfaStatus.NotApplicable, "corr-5");
        TenantContext tenantContext = TenantContext.Unscoped(deniedCaller, "corr-5");

        Func<Task> act = () => PredictiveTenant.EvaluateWithTenantEnforcementAsync(
            BuildRequest(), deniedCaller, tenantContext, requestedFacilityId: null, new AlwaysDenyPermissionResolver(), tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private sealed class AlwaysDenyPermissionResolver : IPermissionResolver
    {
        public IReadOnlySet<string> Resolve(EnterpriseRole role, string moduleName) => new HashSet<string>();
        public IReadOnlySet<string> ResolveAcrossModules(EnterpriseRole role, IEnumerable<string> moduleNames) => new HashSet<string>();
    }
}
