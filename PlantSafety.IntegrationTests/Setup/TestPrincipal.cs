// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         IntegrationTests.Setup
//  Layer:          IntegrationTests
//  File:           TestPrincipal.cs
//  Created:        2026-08-06
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Single source of truth for the fixed TestIds-backed ClaimsPrincipal used by
//                  ServiceProviderExtensions.GetScopedService (direct-DI tests that resolve
//                  ISender straight from the container, bypassing HTTP/middleware entirely --
//                  the majority of this suite's tests use that pattern, not Client). Extracted to
//                  its own type in case a future HTTP-based authenticated-Client need arises and
//                  wants the same identity without duplicating the claim set.
// ============================================================================

using System.Security.Claims;
using PlantSafety.IntegrationTests.Common.TestConstants;

namespace PlantSafety.IntegrationTests.Setup;

/// <summary>Builds the fixed <see cref="TestIds"/>-backed <see cref="ClaimsPrincipal"/>.</summary>
public static class TestPrincipal
{
    /// <summary>Authentication type used for the identity backing the returned principal.</summary>
    public const string AuthenticationType = "TestAuth";

    /// <summary>
    /// Builds a <see cref="ClaimsPrincipal"/> for the fixed <see cref="TestIds"/> identity.
    /// Claim types are deliberately duplicated (camelCase AND snake_case) because this codebase's
    /// two claim readers disagree: TenantResolutionMiddleware reads "tenantId"/"siteId"
    /// (camelCase) while CurrentUserService reads "tenant_id"/"site_id" (snake_case) -- a
    /// pre-existing inconsistency, not something to paper over by picking one.
    /// </summary>
    public static ClaimsPrincipal Create()
    {
        var claims = new[]
        {
            new Claim("sub", TestIds.ActorId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, TestIds.ActorId.ToString()),
            new Claim("tenantId", TestIds.TenantId.ToString()),
            new Claim("tenant_id", TestIds.TenantId.ToString()),
            new Claim("siteId", TestIds.SiteId.ToString()),
            new Claim("site_id", TestIds.SiteId.ToString()),
            new Claim("role", "Platform.Admin"),
            new Claim(ClaimTypes.Role, "Platform.Admin"),
        };

        var identity = new ClaimsIdentity(claims, AuthenticationType);
        return new ClaimsPrincipal(identity);
    }
}
