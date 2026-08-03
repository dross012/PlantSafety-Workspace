// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Unit tests for TenantRegionAffinityResolver (Phase 7 §4.3), verifying
//   determinism, explicit-override precedence, and even-ish distribution across regions for
//   the hash-based fallback.
// SchemaVersion: 1

using Microsoft.Extensions.Options;
using PlantSafety.Application.Enterprise.MultiRegion;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;
using Xunit;

namespace PlantSafety.Tests.Application.Enterprise.MultiRegion;

/// <summary>Tests for <see cref="TenantRegionAffinityResolver"/>.</summary>
public sealed class TenantRegionAffinityResolverTests
{
    private static TenantRegionAffinityResolver CreateSut(IReadOnlyDictionary<string, string>? overrides = null)
    {
        var options = new TenantRegionAffinityOptions
        {
            Overrides = overrides ?? new Dictionary<string, string>()
        };
        return new TenantRegionAffinityResolver(Options.Create(options));
    }

    [Fact]
    public void ResolveRegion_SameTenant_AlwaysReturnsSameRegion()
    {
        var sut = CreateSut();
        var tenantId = Guid.NewGuid();

        var first = sut.ResolveRegion(tenantId);
        var second = sut.ResolveRegion(tenantId);
        var third = sut.ResolveRegion(tenantId);

        Assert.Same(first, second);
        Assert.Same(second, third);
    }

    [Fact]
    public void ResolveRegion_EmptyTenantId_Throws()
    {
        var sut = CreateSut();

        Assert.Throws<ArgumentException>(() => sut.ResolveRegion(Guid.Empty));
    }

    [Fact]
    public void ResolveRegion_ExplicitOverride_TakesPrecedenceOverHash()
    {
        var tenantId = Guid.NewGuid();
        var sut = CreateSut(new Dictionary<string, string>
        {
            [tenantId.ToString("D")] = "Eu"
        });

        var resolved = sut.ResolveRegion(tenantId);

        Assert.Equal(Region.Eu, resolved);
    }

    [Fact]
    public void ResolveRegion_UnknownOverrideCode_FallsBackToHash()
    {
        var tenantId = Guid.NewGuid();
        var sutWithBadOverride = CreateSut(new Dictionary<string, string>
        {
            [tenantId.ToString("D")] = "NotARealRegion"
        });
        var sutWithoutOverride = CreateSut();

        Assert.Equal(sutWithoutOverride.ResolveRegion(tenantId), sutWithBadOverride.ResolveRegion(tenantId));
    }

    [Fact]
    public void ResolveRegion_ManyTenants_DistributesAcrossAllRegions()
    {
        // Not a hard per-bucket guarantee, but across many tenants we expect every region to be
        // used at least once -- guards against a degenerate hash implementation that always
        // resolves to the same region.
        var sut = CreateSut();
        var seenRegions = new HashSet<Region>();

        for (var i = 0; i < 200; i++)
        {
            seenRegions.Add(sut.ResolveRegion(Guid.NewGuid()));
        }

        Assert.Equal(Region.All.Count, seenRegions.Count);
    }
}
