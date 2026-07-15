// ============================================================================
//  Test IDs:  PLNT-AUTH-CLAIMS-001..004
//  Manual:    tests_rules_design_manual.md (AUTH type); OWASP Top 10 (A01
//             Broken Access Control, A07 Identification & Auth Failures).
//  Purpose:   Validates PlantSafety.Infrastructure.Identity.CurrentUserService
//             correctly and safely derives identity from JWT claims,
//             defaulting to "denied"/anonymous on missing or malformed claims
//             (zero-trust default).
// ============================================================================
namespace PlantSafety.SecurityTests.Identity;

public sealed class AuthIdentityTests
{
    private static CurrentUserService BuildService(ClaimsPrincipal? principal)
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        if (principal is not null)
            context.User = principal;
        accessor.HttpContext.Returns(principal is null ? null : context);
        return new CurrentUserService(accessor);
    }

    [Fact(DisplayName = "PLNT-AUTH-CLAIMS-001: No HttpContext -> IsAuthenticated is false and identifiers are empty (zero-trust default)")]
    public void NoHttpContext_ShouldDefaultToUnauthenticated()
    {
        var service = BuildService(null);

        service.IsAuthenticated.Should().BeFalse();
        service.UserId.Should().Be(Guid.Empty);
        service.TenantId.Should().Be(Guid.Empty);
        service.Permissions.Should().BeEmpty();
    }

    [Fact(DisplayName = "PLNT-AUTH-CLAIMS-002: Valid sub/tenant_id/permission claims resolve identity and permission checks correctly")]
    public void ValidClaims_ShouldResolveIdentityAndPermissions()
    {
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var identity = new ClaimsIdentity(
        [
            new Claim("sub", userId.ToString()),
            new Claim("tenant_id", tenantId.ToString()),
            new Claim("permission", "alerts.create"),
            new Claim("role", "SafetyOfficer"),
        ], authenticationType: "TestAuth");

        var service = BuildService(new ClaimsPrincipal(identity));

        service.IsAuthenticated.Should().BeTrue();
        service.UserId.Should().Be(userId);
        service.TenantId.Should().Be(tenantId);
        service.HasPermission("alerts.create").Should().BeTrue();
        service.HasPermission("alerts.delete").Should().BeFalse();
        // NOTE: ClaimsPrincipal.IsInRole() only inspects the standard ClaimTypes.Role claim
        // type; CurrentUserService's custom "role" claim is only exposed via the Roles list
        // fallback (used when there is no HttpContext.User at all). This is documented,
        // pre-existing production behavior -- not altered by this test suite.
        service.Roles.Should().Contain("SafetyOfficer");
    }

    [Fact(DisplayName = "PLNT-AUTH-CLAIMS-003: Malformed GUID claims degrade safely to Guid.Empty rather than throwing")]
    public void MalformedGuidClaims_ShouldNotThrow_AndShouldDegradeToEmpty()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim("sub", "not-a-guid"),
            new Claim("tenant_id", "also-not-a-guid"),
        ], authenticationType: "TestAuth");

        var service = BuildService(new ClaimsPrincipal(identity));

        var act = () => { _ = service.UserId; _ = service.TenantId; };
        act.Should().NotThrow();
        service.UserId.Should().Be(Guid.Empty);
        service.TenantId.Should().Be(Guid.Empty);
    }

    [Fact(DisplayName = "PLNT-PEN-CLAIMS-001: Permission check is case-insensitive but does not match substrings (no permission-string injection)")]
    public void HasPermission_ShouldNotMatchOnSubstring()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("permission", "alerts.create"),
        ], authenticationType: "TestAuth");

        var service = BuildService(new ClaimsPrincipal(identity));

        service.HasPermission("ALERTS.CREATE").Should().BeTrue("permission matching must be case-insensitive");
        service.HasPermission("alerts.create.extra").Should().BeFalse("permission matching must not match on substring/prefix injection");
        service.HasPermission("alerts").Should().BeFalse("permission matching must not match on a partial/prefix claim");
    }
}
