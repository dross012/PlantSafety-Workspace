// AUTHOR: Security & Compliance Agent (Phase 7 §5.3)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Unit tests for DataResidencyPolicy -- the Smart Enum modeling a tenant's
//   data-residency requirement (US-only/EU-only/Global).
// SchemaVersion: 1

using PlantSafety.Domain.MultiRegion.DataResidency;
using Xunit;

namespace PlantSafety.Tests.Domain.MultiRegion;

/// <summary>Tests for <see cref="DataResidencyPolicy"/>.</summary>
public sealed class DataResidencyPolicyTests
{
    [Fact]
    public void Global_IsSatisfiedByAnyZone()
    {
        Assert.True(DataResidencyPolicy.Global.IsSatisfiedByZone("US"));
        Assert.True(DataResidencyPolicy.Global.IsSatisfiedByZone("EU"));
        Assert.True(DataResidencyPolicy.Global.IsSatisfiedByZone("APAC"));
    }

    [Fact]
    public void UsOnly_IsSatisfiedOnlyByUsZone()
    {
        Assert.True(DataResidencyPolicy.UsOnly.IsSatisfiedByZone("US"));
        Assert.False(DataResidencyPolicy.UsOnly.IsSatisfiedByZone("EU"));
        Assert.False(DataResidencyPolicy.UsOnly.IsSatisfiedByZone("APAC"));
    }

    [Fact]
    public void EuOnly_IsSatisfiedOnlyByEuZone()
    {
        Assert.True(DataResidencyPolicy.EuOnly.IsSatisfiedByZone("EU"));
        Assert.False(DataResidencyPolicy.EuOnly.IsSatisfiedByZone("US"));
        Assert.False(DataResidencyPolicy.EuOnly.IsSatisfiedByZone("APAC"));
    }

    [Fact]
    public void IsSatisfiedByZone_IsCaseInsensitive()
    {
        Assert.True(DataResidencyPolicy.EuOnly.IsSatisfiedByZone("eu"));
    }

    [Fact]
    public void Default_IsGlobal()
    {
        Assert.Same(DataResidencyPolicy.Global, DataResidencyPolicy.Default);
    }

    [Theory]
    [InlineData("UsOnly")]
    [InlineData("EuOnly")]
    [InlineData("Global")]
    [InlineData("usonly")]
    public void TryParse_KnownCode_ResolvesCaseInsensitively(string code)
    {
        Assert.True(DataResidencyPolicy.TryParse(code, out var value));
        Assert.NotNull(value);
    }

    [Fact]
    public void TryParse_UnknownCode_ReturnsFalse()
    {
        Assert.False(DataResidencyPolicy.TryParse("NotARealPolicy", out var value));
        Assert.Null(value);
    }

    [Fact]
    public void From_UnknownCode_Throws()
    {
        Assert.Throws<ArgumentException>(() => DataResidencyPolicy.From("NotARealPolicy"));
    }

    [Fact]
    public void All_ContainsExactlyThreeCanonicalPolicies()
    {
        Assert.Equal(3, DataResidencyPolicy.All.Count);
        Assert.Contains(DataResidencyPolicy.UsOnly, DataResidencyPolicy.All);
        Assert.Contains(DataResidencyPolicy.EuOnly, DataResidencyPolicy.All);
        Assert.Contains(DataResidencyPolicy.Global, DataResidencyPolicy.All);
    }
}
