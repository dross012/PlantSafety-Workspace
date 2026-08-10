// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           TenantIsolationCheckTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;

using Xunit;

namespace Shared.Authorization.Tests.Tenant;

public sealed class TenantIsolationCheckTests
{
    private sealed class FakeResolver(bool canAccess) : ITenantFacilityAccessResolver
    {
        public bool CanAccessFacility(string tenantId, Guid facilityId) => canAccess;
    }

    private static readonly CallerContext Caller = new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

    [Fact]
    public void Evaluate_NullRequestedFacility_AlwaysAllows()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-1");

        TenantIsolationCheck.Evaluate(tenantContext, requestedFacilityId: null, tenantFacilityAccessResolver: null).IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_BoundFacilityMatchesRequest_Allows()
    {
        Guid facilityId = Guid.NewGuid();
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: facilityId, Caller, "corr-2");

        TenantIsolationCheck.Evaluate(tenantContext, facilityId, tenantFacilityAccessResolver: null).IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_BoundFacilityMismatch_Denies()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: Guid.NewGuid(), Caller, "corr-3");

        TenantIsolationCheck.Evaluate(tenantContext, Guid.NewGuid(), tenantFacilityAccessResolver: null).IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_UnboundTenantWithResolverGranting_Allows()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-4");

        TenantIsolationCheck.Evaluate(tenantContext, Guid.NewGuid(), new FakeResolver(true)).IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_UnboundTenantWithResolverDenying_Denies()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-5");

        TenantIsolationCheck.Evaluate(tenantContext, Guid.NewGuid(), new FakeResolver(false)).IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_UnboundTenantWithNoResolver_Denies()
    {
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-6");

        TenantIsolationCheck.Evaluate(tenantContext, Guid.NewGuid(), tenantFacilityAccessResolver: null).IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_UnscopedTenant_WithNullFacility_Allows()
    {
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-7");

        TenantIsolationCheck.Evaluate(tenantContext, requestedFacilityId: null, tenantFacilityAccessResolver: null).IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_UnscopedTenant_WithRealFacilityAndNoResolver_StillAllows()
    {
        // TenantContext.Unscoped must always allow, even when the caller's upstream data already
        // carries a real, non-null requestedFacilityId and no resolver was supplied -- distinct from
        // a named tenant with FacilityId: null, which is NOT automatically allowed (see the
        // UnboundTenantWithNoResolver_Denies test above).
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-8");

        TenantIsolationCheck.Evaluate(tenantContext, Guid.NewGuid(), tenantFacilityAccessResolver: null).IsAuthorized.Should().BeTrue();
    }
}
