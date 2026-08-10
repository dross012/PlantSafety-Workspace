// ============================================================================
//  Project:        Shared.SafetyData.Tests
//  File:           RoutingTenantTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;
using Shared.Deterministic.Abstractions.Clock;
using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;
using Shared.SafetyData.Infrastructure.Tenant;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.SafetyData.Tests.Tenant;

public sealed class RoutingTenantTests
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

    private static (FacilityProfile facility, EventDefinition eventDefinition) MakeInputs(Guid? facilityId = null)
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(facilityId ?? Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        EventDefinition eventDefinition = EventCatalog.FindByCode("ChemicalSpill")!;
        return (facility, eventDefinition);
    }

    [Fact]
    public async Task RouteWithTenantEnforcementAsync_UnscopedTenant_MatchesDirectEngineResult()
    {
        (FacilityProfile facility, EventDefinition eventDefinition) = MakeInputs();
        TenantContext tenantContext = TenantContext.Unscoped(Caller, "corr-1");

        EventRoutingResult viaWrapper = await RoutingTenant.RouteWithTenantEnforcementAsync(
            eventDefinition, facility, Caller, tenantContext, Resolver, tenantFacilityAccessResolver: null, Clock, Writer);
        EventRoutingResult direct = EventRoutingEngine.Route(eventDefinition, facility);

        viaWrapper.Should().BeEquivalentTo(direct);
    }

    [Fact]
    public async Task RouteWithTenantEnforcementAsync_TenantScopedToSameFacility_Succeeds()
    {
        Guid facilityId = Guid.NewGuid();
        (FacilityProfile facility, EventDefinition eventDefinition) = MakeInputs(facilityId);
        var tenantContext = new TenantContext("acme-plant-1", facilityId, Caller, "corr-2");

        Func<Task> act = () => RoutingTenant.RouteWithTenantEnforcementAsync(
            eventDefinition, facility, Caller, tenantContext, Resolver, tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RouteWithTenantEnforcementAsync_TenantScopedToDifferentFacility_ThrowsUnauthorizedAccessException()
    {
        (FacilityProfile facility, EventDefinition eventDefinition) = MakeInputs();
        var tenantContext = new TenantContext("acme-plant-1", Guid.NewGuid(), Caller, "corr-3");

        Func<Task> act = () => RoutingTenant.RouteWithTenantEnforcementAsync(
            eventDefinition, facility, Caller, tenantContext, Resolver, tenantFacilityAccessResolver: null, Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task RouteWithTenantEnforcementAsync_ResolverGrantsAccess_Succeeds()
    {
        (FacilityProfile facility, EventDefinition eventDefinition) = MakeInputs();
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-4");

        Func<Task> act = () => RoutingTenant.RouteWithTenantEnforcementAsync(
            eventDefinition, facility, Caller, tenantContext, Resolver, new FakeResolver(true), Clock, Writer);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RouteWithTenantEnforcementAsync_ResolverDeniesAccess_ThrowsUnauthorizedAccessException()
    {
        (FacilityProfile facility, EventDefinition eventDefinition) = MakeInputs();
        var tenantContext = new TenantContext("acme-plant-1", FacilityId: null, Caller, "corr-5");

        Func<Task> act = () => RoutingTenant.RouteWithTenantEnforcementAsync(
            eventDefinition, facility, Caller, tenantContext, Resolver, new FakeResolver(false), Clock, Writer);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
