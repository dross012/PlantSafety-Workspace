// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           TenantContextFactoryTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Roles;
using Shared.Authorization.Abstractions.Tenant;

using Xunit;

namespace Shared.Authorization.Tests.Tenant;

public sealed class TenantContextFactoryTests
{
    private static readonly CallerContext Caller = new(EnterpriseRole.Editor, MfaStatus.NotApplicable, "corr-1");

    private static readonly IReadOnlyDictionary<string, string> RegisteredKeyTenants = new Dictionary<string, string>
    {
        ["tenant-key"] = "acme-plant-1",
    };

    [Fact]
    public void Create_NoKeyProvided_FallsBackToUnscoped()
    {
        TenantContextResolution resolution = TenantContextFactory.Create(null, RegisteredKeyTenants, Caller, "corr-1");

        resolution.Context.TenantId.Should().Be(TenantContext.UnscopedTenantId);
        resolution.TenantKeyProvided.Should().BeFalse();
        resolution.TenantRecognized.Should().BeFalse();
    }

    [Fact]
    public void Create_UnrecognizedKey_FallsBackToUnscopedButFlagsKeyProvided()
    {
        TenantContextResolution resolution = TenantContextFactory.Create("bogus", RegisteredKeyTenants, Caller, "corr-2");

        resolution.Context.TenantId.Should().Be(TenantContext.UnscopedTenantId);
        resolution.TenantKeyProvided.Should().BeTrue();
        resolution.TenantRecognized.Should().BeFalse();
    }

    [Fact]
    public void Create_RecognizedKey_ResolvesMappedTenantId()
    {
        TenantContextResolution resolution = TenantContextFactory.Create("tenant-key", RegisteredKeyTenants, Caller, "corr-3");

        resolution.Context.TenantId.Should().Be("acme-plant-1");
        resolution.Context.FacilityId.Should().BeNull();
        resolution.Context.Caller.Should().Be(Caller);
        resolution.Context.CorrelationId.Should().Be("corr-3");
        resolution.TenantKeyProvided.Should().BeTrue();
        resolution.TenantRecognized.Should().BeTrue();
    }
}
