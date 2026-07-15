// ============================================================================
//  Test IDs:  PLNT-SEC-TENANT-001..003
//  Manual:    tests_rules_design_manual.md (SEC type); NIST SP 800-53
//             (Azure/PlantSafety); OWASP A01 Broken Access Control.
//  Purpose:   Confirms PermissionService enforces strict tenant isolation --
//             a request can only be evaluated against its OWN authenticated
//             identity/tenant, never against another user's or tenant's
//             permissions (horizontal privilege-escalation prevention).
// ============================================================================
namespace PlantSafety.SecurityTests.Identity;

public sealed class TenantIsolationTests
{
    private static PermissionService BuildService(Guid authenticatedUserId, Guid authenticatedTenantId, params string[] permissions)
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns(authenticatedUserId);
        currentUser.TenantId.Returns(authenticatedTenantId);
        currentUser.HasPermission(Arg.Any<string>()).Returns(ci => permissions.Contains((string)ci[0]));

        return new PermissionService(currentUser, NullLogger<PermissionService>.Instance);
    }

    [Fact(DisplayName = "PLNT-SEC-TENANT-001: Permission check for the authenticated user's own tenant is granted when the claim is present")]
    public async Task HasPermission_ForOwnUserAndTenant_ShouldBeGranted()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var service = BuildService(userId, tenantId, "incidents.read");

        var result = await service.HasPermissionAsync(userId, tenantId, "incidents.read");

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "PLNT-SEC-TENANT-002: Permission check for a DIFFERENT tenant is always denied, even if the permission string matches (tenant isolation)")]
    public async Task HasPermission_ForDifferentTenant_ShouldAlwaysBeDenied()
    {
        var userId = Guid.NewGuid();
        var ownTenant = Guid.NewGuid();
        var otherTenant = Guid.NewGuid();
        var service = BuildService(userId, ownTenant, "incidents.read");

        var result = await service.HasPermissionAsync(userId, otherTenant, "incidents.read");

        result.Should().BeFalse("a caller must never be able to assert permissions against a tenant other than their own");
    }

    [Fact(DisplayName = "PLNT-SEC-TENANT-003: Permission check for a DIFFERENT user id is always denied (no impersonation via id substitution)")]
    public async Task HasPermission_ForDifferentUserId_ShouldAlwaysBeDenied()
    {
        var authenticatedUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var service = BuildService(authenticatedUserId, tenantId, "incidents.read");

        var result = await service.HasPermissionAsync(targetUserId, tenantId, "incidents.read");

        result.Should().BeFalse("PermissionService only evaluates the caller's own claims and must reject id-substitution attempts");
    }

    [Fact(DisplayName = "PLNT-SEC-TENANT-004: HasAllPermissionsAsync denies when any required permission is missing")]
    public async Task HasAllPermissions_ShouldDeny_WhenAnyPermissionMissing()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var service = BuildService(userId, tenantId, "incidents.read");

        var result = await service.HasAllPermissionsAsync(userId, tenantId, ["incidents.read", "incidents.delete"]);

        result.Should().BeFalse();
    }
}
