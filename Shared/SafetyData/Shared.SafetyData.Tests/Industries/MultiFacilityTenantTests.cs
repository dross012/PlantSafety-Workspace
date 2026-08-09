// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           MultiFacilityTenantTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.8 (Path C: Operational Modernization) unit tests
//                  for TenantFacilityRegistryV2: multi-facility binding,
//                  primary designation/promotion, routing, and dashboard
//                  ordering.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries.Orchestration;
using Shared.SafetyData.Infrastructure.Industries;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class MultiFacilityTenantTests
{
    [Fact]
    public void RegisterFacility_FirstFacility_BecomesPrimary()
    {
        var registry = new TenantFacilityRegistryV2();
        var facilityId = Guid.NewGuid();

        registry.RegisterFacility("tenant-1", facilityId);

        Assert.True(registry.TryResolvePrimaryFacility("tenant-1", out Guid primary));
        Assert.Equal(facilityId, primary);
    }

    [Fact]
    public void RegisterFacility_SecondFacility_DoesNotDisplacePrimaryUnlessRequested()
    {
        var registry = new TenantFacilityRegistryV2();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        registry.RegisterFacility("tenant-1", first);

        registry.RegisterFacility("tenant-1", second);

        Assert.True(registry.TryResolvePrimaryFacility("tenant-1", out Guid primary));
        Assert.Equal(first, primary);
        Assert.True(registry.TryGetFacilities("tenant-1", out IReadOnlyList<Guid> facilities));
        Assert.Equal(2, facilities.Count);
    }

    [Fact]
    public void RegisterFacility_ExplicitPrimary_Overrides()
    {
        var registry = new TenantFacilityRegistryV2();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        registry.RegisterFacility("tenant-1", first);

        registry.RegisterFacility("tenant-1", second, isPrimary: true);

        Assert.True(registry.TryResolvePrimaryFacility("tenant-1", out Guid primary));
        Assert.Equal(second, primary);
    }

    [Fact]
    public void RegisterFacility_EmptyTenantId_Throws()
    {
        var registry = new TenantFacilityRegistryV2();

        Assert.Throws<ArgumentException>(() => registry.RegisterFacility("", Guid.NewGuid()));
    }

    [Fact]
    public void RegisterFacility_EmptyFacilityId_Throws()
    {
        var registry = new TenantFacilityRegistryV2();

        Assert.Throws<ArgumentException>(() => registry.RegisterFacility("tenant-1", Guid.Empty));
    }

    [Fact]
    public void UnregisterFacility_RemovesBinding()
    {
        var registry = new TenantFacilityRegistryV2();
        var facilityId = Guid.NewGuid();
        registry.RegisterFacility("tenant-1", facilityId);

        bool removed = registry.UnregisterFacility("tenant-1", facilityId);

        Assert.True(removed);
        Assert.False(registry.TryResolvePrimaryFacility("tenant-1", out _));
    }

    [Fact]
    public void UnregisterFacility_RemovingPrimary_PromotesRemainingFacility()
    {
        var registry = new TenantFacilityRegistryV2();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        registry.RegisterFacility("tenant-1", first);
        registry.RegisterFacility("tenant-1", second);

        registry.UnregisterFacility("tenant-1", first);

        Assert.True(registry.TryResolvePrimaryFacility("tenant-1", out Guid primary));
        Assert.Equal(second, primary);
    }

    [Fact]
    public void UnregisterFacility_UnknownBinding_ReturnsFalse()
    {
        var registry = new TenantFacilityRegistryV2();

        Assert.False(registry.UnregisterFacility("tenant-1", Guid.NewGuid()));
    }

    [Fact]
    public void CanRouteToFacility_KnownBinding_ReturnsTrue()
    {
        var registry = new TenantFacilityRegistryV2();
        var facilityId = Guid.NewGuid();
        registry.RegisterFacility("tenant-1", facilityId);

        Assert.True(registry.CanRouteToFacility("tenant-1", facilityId));
    }

    [Fact]
    public void CanRouteToFacility_UnknownBinding_ReturnsFalse()
    {
        var registry = new TenantFacilityRegistryV2();
        registry.RegisterFacility("tenant-1", Guid.NewGuid());

        Assert.False(registry.CanRouteToFacility("tenant-1", Guid.NewGuid()));
    }

    [Fact]
    public void GetDashboardFacilityIds_PrimaryFacilityIsFirst()
    {
        var registry = new TenantFacilityRegistryV2();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var third = Guid.NewGuid();
        registry.RegisterFacility("tenant-1", first);
        registry.RegisterFacility("tenant-1", second);
        registry.RegisterFacility("tenant-1", third, isPrimary: true);

        IReadOnlyList<Guid> dashboardOrder = registry.GetDashboardFacilityIds("tenant-1");

        Assert.Equal(third, dashboardOrder[0]);
        Assert.Equal(3, dashboardOrder.Count);
    }

    [Fact]
    public void GetAll_ReturnsEveryRegisteredTenantMap()
    {
        var registry = new TenantFacilityRegistryV2();
        registry.RegisterFacility("tenant-1", Guid.NewGuid());
        registry.RegisterFacility("tenant-2", Guid.NewGuid());

        IReadOnlyList<TenantFacilityMap> all = registry.GetAll();

        Assert.Equal(2, all.Count);
    }
}
