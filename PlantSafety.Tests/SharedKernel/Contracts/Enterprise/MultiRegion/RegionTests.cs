// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Unit tests for the Region Smart Enum (Phase 7 §4.1).
// SchemaVersion: 1

using PlantSafety.SharedKernel.Contracts.Enterprise.MultiRegion;
using Xunit;

namespace PlantSafety.Tests.SharedKernel.Contracts.Enterprise.MultiRegion;

/// <summary>Tests for <see cref="Region"/>.</summary>
public sealed class RegionTests
{
    [Theory]
    [InlineData("UsEast")]
    [InlineData("UsWest")]
    [InlineData("Eu")]
    [InlineData("Apac")]
    public void From_KnownCode_ResolvesRegion(string code)
    {
        var region = Region.From(code);

        Assert.Equal(code, region.Code);
    }

    [Fact]
    public void From_UnknownCode_Throws()
    {
        Assert.Throws<ArgumentException>(() => Region.From("Antarctica"));
    }

    [Fact]
    public void TryParse_UnknownCode_ReturnsFalse()
    {
        var result = Region.TryParse("NotARegion", out var region);

        Assert.False(result);
        Assert.Null(region);
    }

    [Fact]
    public void All_ContainsExactlyFourRegions()
    {
        Assert.Equal(4, Region.All.Count);
    }

    [Fact]
    public void Default_IsUsEast()
    {
        Assert.Equal(Region.UsEast, Region.Default);
    }

    [Fact]
    public void IsValid_IsCaseInsensitive()
    {
        Assert.True(Region.IsValid("useast"));
        Assert.True(Region.IsValid("EU"));
    }
}
