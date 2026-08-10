// ============================================================================
//  Project:        Deployment.Tests
//  File:           TenantLifecycleTests.cs
//  Created:        2026-08-10
// ============================================================================
using Deployment.ConfigSchema.Contracts;
using Deployment.Tenancy;

using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Permissions;
using Shared.Authorization.Abstractions.Roles;
using Shared.Deterministic.Abstractions.Clock;

using Xunit;

namespace Deployment.Tests.Unit;

[Trait("Category", "Unit")]
public sealed class TenantLifecycleTests
{
    private static readonly IPermissionResolver Resolver = new EnterprisePermissionResolver();
    private static readonly FixedClock Clock = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static readonly CallerContext AdminCaller = new(EnterpriseRole.PlatformAdmin, MfaStatus.Verified, "corr-1");
    private static readonly CallerContext UnverifiedAdminCaller = new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-2");
    private static readonly CallerContext ViewerCaller = new(EnterpriseRole.Viewer, MfaStatus.Verified, "corr-3");

    private static CreateTenantRequest BuildCreateRequest(string tenantId = "acme-plant-1") => new(
        tenantId, "Acme Plant 1", TenantTopology.Shared, new TenantResourceQuota(10, 5, 1_000_000), Array.Empty<string>(), Array.Empty<string>());

    [Fact]
    public async Task CreateTenantAsync_AdminWithVerifiedMfa_Succeeds()
    {
        var service = new TenantService(Resolver, Clock);

        TenantLifecycleResult result = await service.CreateTenantAsync(BuildCreateRequest(), AdminCaller);

        result.Succeeded.Should().BeTrue();
        result.Tenant!.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public async Task CreateTenantAsync_AdminWithoutVerifiedMfa_Fails()
    {
        var service = new TenantService(Resolver, Clock);

        TenantLifecycleResult result = await service.CreateTenantAsync(BuildCreateRequest(), UnverifiedAdminCaller);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTenantAsync_ViewerRole_Fails()
    {
        var service = new TenantService(Resolver, Clock);

        TenantLifecycleResult result = await service.CreateTenantAsync(BuildCreateRequest(), ViewerCaller);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTenantAsync_DuplicateTenantId_Fails()
    {
        var service = new TenantService(Resolver, Clock);
        await service.CreateTenantAsync(BuildCreateRequest(), AdminCaller);

        TenantLifecycleResult result = await service.CreateTenantAsync(BuildCreateRequest(), AdminCaller);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTenantAsync_ExistingActiveTenant_UpdatesName()
    {
        var service = new TenantService(Resolver, Clock);
        await service.CreateTenantAsync(BuildCreateRequest(), AdminCaller);

        TenantLifecycleResult result = await service.UpdateTenantAsync(new UpdateTenantRequest("acme-plant-1", "New Name", null), AdminCaller);

        result.Succeeded.Should().BeTrue();
        result.Tenant!.TenantName.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateTenantAsync_NonexistentTenant_Fails()
    {
        var service = new TenantService(Resolver, Clock);

        TenantLifecycleResult result = await service.UpdateTenantAsync(new UpdateTenantRequest("missing", "x", null), AdminCaller);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task SuspendTenantAsync_ActiveTenant_SetsSuspendedStatus()
    {
        var service = new TenantService(Resolver, Clock);
        await service.CreateTenantAsync(BuildCreateRequest(), AdminCaller);

        TenantLifecycleResult result = await service.SuspendTenantAsync(new SuspendTenantRequest("acme-plant-1", "policy violation"), AdminCaller);

        result.Succeeded.Should().BeTrue();
        result.Tenant!.Status.Should().Be(TenantStatus.Suspended);
    }

    [Fact]
    public async Task DeleteTenantAsync_ActiveTenant_SetsDeletedStatus()
    {
        var service = new TenantService(Resolver, Clock);
        await service.CreateTenantAsync(BuildCreateRequest(), AdminCaller);

        TenantLifecycleResult result = await service.DeleteTenantAsync(new DeleteTenantRequest("acme-plant-1", "offboarded"), AdminCaller);

        result.Succeeded.Should().BeTrue();
        result.Tenant!.Status.Should().Be(TenantStatus.Deleted);
    }

    [Fact]
    public async Task DeleteTenantAsync_AlreadyDeleted_Fails()
    {
        var service = new TenantService(Resolver, Clock);
        await service.CreateTenantAsync(BuildCreateRequest(), AdminCaller);
        await service.DeleteTenantAsync(new DeleteTenantRequest("acme-plant-1", "first delete"), AdminCaller);

        TenantLifecycleResult result = await service.DeleteTenantAsync(new DeleteTenantRequest("acme-plant-1", "second delete"), AdminCaller);

        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTenantAsync_SuspendedTenant_Fails()
    {
        var service = new TenantService(Resolver, Clock);
        await service.CreateTenantAsync(BuildCreateRequest(), AdminCaller);
        await service.SuspendTenantAsync(new SuspendTenantRequest("acme-plant-1", "reason"), AdminCaller);

        TenantLifecycleResult result = await service.UpdateTenantAsync(new UpdateTenantRequest("acme-plant-1", "New Name", null), AdminCaller);

        result.Succeeded.Should().BeFalse();
    }
}
