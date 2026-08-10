// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           TenantContextFactoryTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;

using Xunit;

namespace Shared.Authorization.Tests.Tenant;

public sealed class TenantContextFactoryTests
{
    private static readonly CallerContext Caller = new(EnterpriseRole.PlatformAdmin, MfaStatus.NotApplicable, "corr-1");

    private static readonly IReadOnlyDictionary<string, string> RegisteredKeyTenants = new Dictionary<string, string>
    {
        ["key-acme"] = "acme-plant-1",
    };

    [Fact]
    public void Create_NoHeaderValue_FallsBackToUnscoped()
    {
        TenantContextResolution resolution = TenantContextFactory.Create(null, RegisteredKeyTenants, Caller, "corr-1");

        resolution.Outcome.Should().Be(TenantContextResolutionOutcome.NoKeyProvided);
        resolution.Context.TenantId.Should().Be(TenantContext.UnscopedTenantId);
    }

    [Fact]
    public void Create_RecognizedKey_ResolvesToRegisteredTenant()
    {
        TenantContextResolution resolution = TenantContextFactory.Create("key-acme", RegisteredKeyTenants, Caller, "corr-2");

        resolution.Outcome.Should().Be(TenantContextResolutionOutcome.Recognized);
        resolution.Context.TenantId.Should().Be("acme-plant-1");
        resolution.Context.FacilityId.Should().BeNull();
    }

    [Fact]
    public void Create_UnrecognizedKey_FallsBackToUnscoped()
    {
        TenantContextResolution resolution = TenantContextFactory.Create("nonexistent-key", RegisteredKeyTenants, Caller, "corr-3");

        resolution.Outcome.Should().Be(TenantContextResolutionOutcome.Unrecognized);
        resolution.Context.TenantId.Should().Be(TenantContext.UnscopedTenantId);
    }
}
