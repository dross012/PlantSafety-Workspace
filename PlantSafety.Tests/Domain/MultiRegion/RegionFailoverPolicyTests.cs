// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Unit tests for RegionFailoverPolicy (Phase 7 §4.3/§4.4), verifying primary
//   partner selection, fallback to any healthy region, and the "no healthy region" case.
// SchemaVersion: 1

using PlantSafety.Domain.MultiRegion.Services;
using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;
using Xunit;

namespace PlantSafety.Tests.Domain.MultiRegion;

/// <summary>Tests for <see cref="RegionFailoverPolicy"/>.</summary>
public sealed class RegionFailoverPolicyTests
{
    private readonly RegionFailoverPolicy _sut = new();

    private static Dictionary<Region, RegionHealthStatus> AllHealthy() => new()
    {
        [Region.UsEast] = RegionHealthStatus.Healthy,
        [Region.UsWest] = RegionHealthStatus.Healthy,
        [Region.Eu] = RegionHealthStatus.Healthy,
        [Region.Apac] = RegionHealthStatus.Healthy,
    };

    [Fact]
    public void DetermineFailoverTarget_PrimaryPartnerHealthy_ReturnsPrimaryPartner()
    {
        var health = AllHealthy();

        Assert.Equal(Region.UsWest, _sut.DetermineFailoverTarget(Region.UsEast, health));
        Assert.Equal(Region.UsEast, _sut.DetermineFailoverTarget(Region.UsWest, health));
        Assert.Equal(Region.Apac, _sut.DetermineFailoverTarget(Region.Eu, health));
        Assert.Equal(Region.Eu, _sut.DetermineFailoverTarget(Region.Apac, health));
    }

    [Fact]
    public void DetermineFailoverTarget_PrimaryPartnerUnavailable_FallsBackToFirstHealthyRegion()
    {
        var health = AllHealthy();
        health[Region.UsWest] = RegionHealthStatus.Unavailable;

        // UsEast's primary partner (UsWest) is unavailable -- falls back to the first healthy
        // region in canonical order, excluding UsEast itself: Eu.
        var result = _sut.DetermineFailoverTarget(Region.UsEast, health);

        Assert.Equal(Region.Eu, result);
    }

    [Fact]
    public void DetermineFailoverTarget_NoOtherRegionHealthy_ReturnsNull()
    {
        var health = new Dictionary<Region, RegionHealthStatus>
        {
            [Region.UsEast] = RegionHealthStatus.Unavailable,
            [Region.UsWest] = RegionHealthStatus.Degraded,
            [Region.Eu] = RegionHealthStatus.Unavailable,
            [Region.Apac] = RegionHealthStatus.Degraded,
        };

        var result = _sut.DetermineFailoverTarget(Region.UsEast, health);

        Assert.Null(result);
    }

    [Fact]
    public void DetermineFailoverTarget_MissingRegionInSnapshot_Throws()
    {
        var incompleteHealth = new Dictionary<Region, RegionHealthStatus>
        {
            [Region.UsEast] = RegionHealthStatus.Healthy,
            [Region.UsWest] = RegionHealthStatus.Healthy,
            [Region.Eu] = RegionHealthStatus.Healthy,
            // Apac deliberately omitted.
        };

        Assert.Throws<ArgumentException>(() => _sut.DetermineFailoverTarget(Region.UsEast, incompleteHealth));
    }

    [Fact]
    public void DetermineFailoverTarget_SameInputs_AlwaysReturnsSameResult()
    {
        var health = AllHealthy();
        health[Region.UsWest] = RegionHealthStatus.Degraded;

        var first = _sut.DetermineFailoverTarget(Region.UsEast, health);
        var second = _sut.DetermineFailoverTarget(Region.UsEast, health);

        Assert.Equal(first, second);
    }
}
