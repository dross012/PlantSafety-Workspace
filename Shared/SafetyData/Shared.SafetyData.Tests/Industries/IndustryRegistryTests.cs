// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           IndustryRegistryTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the core Industry taxonomy: IndustryRegistry,
//                  SafetyFlagCatalog, and Regulator/Regulator.Code(). Verifies
//                  the taxonomy's structural invariants (count, uniqueness,
//                  totality) rather than re-asserting specific business
//                  content, since the taxonomy itself is the source of truth.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class IndustryRegistryTests
{
    [Fact]
    public void All_ContainsExactlyFortyIndustries()
    {
        Assert.Equal(40, IndustryRegistry.All.Count);
    }

    [Fact]
    public void All_HasUniqueIndustryNames()
    {
        var distinctNames = IndustryRegistry.All.Select(industry => industry.Name).Distinct(StringComparer.Ordinal).Count();

        Assert.Equal(IndustryRegistry.All.Count, distinctNames);
    }

    [Fact]
    public void All_EveryIndustryHasAtLeastOneRegulatorAndOneFlag()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            Assert.NotEmpty(industry.Regulators);
            Assert.NotEqual(SafetyFlag.None, industry.Flags);
        }
    }

    [Fact]
    public void All_SubIndustriesIsNeverNull()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            Assert.NotNull(industry.SubIndustries);
        }
    }

    [Theory]
    [InlineData("Chemical Manufacturing", "Core Manufacturing")]
    [InlineData("Warehousing & Distribution", "Logistics & Transport")]
    [InlineData("Explosives & Pyrotechnics", "High Fire/Explosion Risk")]
    [InlineData("Hazardous Waste Treatment", "High Chemical Hazard")]
    [InlineData("Mining & Mineral Processing", "Heavy Machinery / Industrial")]
    [InlineData("Marine Terminals", "Water/Marine Interface")]
    [InlineData("Power Generation", "High Environmental Compliance")]
    [InlineData("Cannabis", "Specialized")]
    public void FindByName_ReturnsIndustryWithExpectedTopLevelCategory(string name, string expectedCategory)
    {
        IndustryDefinition? industry = IndustryRegistry.FindByName(name);

        Assert.NotNull(industry);
        Assert.Equal(expectedCategory, industry!.TopLevelCategory);
    }

    [Fact]
    public void FindByName_UnknownName_ReturnsNull()
    {
        Assert.Null(IndustryRegistry.FindByName("Not A Real Industry"));
    }

    [Fact]
    public void ByTopLevelCategory_PartitionsAllIndustriesExactlyOnce()
    {
        var categories = IndustryRegistry.All.Select(industry => industry.TopLevelCategory).Distinct(StringComparer.Ordinal);

        int totalAcrossCategories = categories.Sum(category => IndustryRegistry.ByTopLevelCategory(category).Count);

        Assert.Equal(IndustryRegistry.All.Count, totalAcrossCategories);
    }

    [Fact]
    public void SafetyFlagCatalog_AllFlagsContainsExactlyTenSingleBitValues()
    {
        Assert.Equal(10, SafetyFlagCatalog.AllFlags.Count);
        foreach (SafetyFlag flag in SafetyFlagCatalog.AllFlags)
        {
            Assert.True(BitOperations_IsPowerOfTwo((int)flag));
        }
    }

    [Fact]
    public void SafetyFlagCatalog_Decompose_ReconstitutesTheOriginalCombinedFlag()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            SafetyFlag recombined = SafetyFlagCatalog.Decompose(industry.Flags)
                .Aggregate(SafetyFlag.None, (accumulated, flag) => accumulated | flag);

            Assert.Equal(industry.Flags, recombined);
        }
    }

    [Fact]
    public void SafetyFlagCatalog_Decompose_None_ReturnsEmpty()
    {
        Assert.Empty(SafetyFlagCatalog.Decompose(SafetyFlag.None));
    }

    [Fact]
    public void Regulator_Code_IsTotalAndNonEmptyForEveryRegulator()
    {
        foreach (Regulator regulator in Enum.GetValues<Regulator>())
        {
            string code = regulator.Code();
            Assert.False(string.IsNullOrWhiteSpace(code));
        }
    }

    [Theory]
    [InlineData(Regulator.EPA_RCRA, "EPA-RCRA")]
    [InlineData(Regulator.EPA_TSCA, "EPA-TSCA")]
    [InlineData(Regulator.NFPA_Dust, "NFPA-Dust")]
    [InlineData(Regulator.DHS_CFATS, "DHS-CFATS")]
    [InlineData(Regulator.ANSI_ISO, "ANSI-ISO")]
    public void Regulator_Code_PreservesHyphenatedNamesLostToEnumIdentifierRules(Regulator regulator, string expectedCode)
    {
        Assert.Equal(expectedCode, regulator.Code());
    }

    private static bool BitOperations_IsPowerOfTwo(int value) => value > 0 && (value & (value - 1)) == 0;
}
