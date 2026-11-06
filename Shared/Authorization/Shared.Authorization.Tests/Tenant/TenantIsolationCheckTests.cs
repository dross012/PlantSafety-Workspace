// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           TenantIsolationCheckTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

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

    private static readonly CallerContext Caller = new(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-1");

    [Fact]
    public void Evaluate_UnscopedTenant_AlwaysAllows()
    {
        TenantContext context = TenantContext.Unscoped(Caller, "corr-1");

        TenantIsolationCheck.Evaluate(context, Guid.NewGuid(), new FakeResolver(false)).IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_NullRequestedFacility_AlwaysAllows()
    {
        var context = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-1");

        TenantIsolationCheck.Evaluate(context, null, new FakeResolver(false)).IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ScopedFacilityMatchesRequested_Allows()
    {
        Guid facilityId = Guid.NewGuid();
        var context = new TenantContext("acme-plant-1", facilityId, Caller, "corr-1");

        TenantIsolationCheck.Evaluate(context, facilityId, resolver: null).IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ScopedFacilityDiffersFromRequested_Denies()
    {
        var context = new TenantContext("acme-plant-1", Guid.NewGuid(), Caller, "corr-1");

        TenantIsolationCheck.Evaluate(context, Guid.NewGuid(), resolver: null).IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_UnscopedFacilityWithResolverGrantingAccess_Allows()
    {
        var context = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-1");

        TenantIsolationCheck.Evaluate(context, Guid.NewGuid(), new FakeResolver(true)).IsAuthorized.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_UnscopedFacilityWithResolverDenyingAccess_Denies()
    {
        var context = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-1");

        TenantIsolationCheck.Evaluate(context, Guid.NewGuid(), new FakeResolver(false)).IsAuthorized.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_UnscopedFacilityWithNoResolver_Denies()
    {
        var context = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-1");

        TenantIsolationCheck.Evaluate(context, Guid.NewGuid(), resolver: null).IsAuthorized.Should().BeFalse();
    }
}
