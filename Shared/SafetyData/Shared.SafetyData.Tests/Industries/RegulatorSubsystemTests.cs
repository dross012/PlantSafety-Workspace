// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           RegulatorSubsystemTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Regulatory Subsystem:
//                  RegulatorCategoryCatalog, RegulatorRequirementCatalog,
//                  RegulatorRequirementMapping, and the five Regulator-first
//                  consumer mappings (RegulatorSafetyModuleMapping,
//                  RegulatorSopRequirements, RegulatorPermitRequirements,
//                  RegulatorAuditRequirements, RegulatorTrainingRequirements).
//                  Verifies totality over all 15 Regulator values (no
//                  exceptions), the documented category-folding rule
//                  (FMCSA/ANSI_ISO -> OSHA, DoD -> ATF), and that
//                  Category is always consistent with
//                  RegulatorCategoryCatalog.ForRegulator.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class RegulatorSubsystemTests
{
    [Theory]
    [InlineData(Regulator.OSHA, RegulatorCategory.OSHA)]
    [InlineData(Regulator.FMCSA, RegulatorCategory.OSHA)]
    [InlineData(Regulator.ANSI_ISO, RegulatorCategory.OSHA)]
    [InlineData(Regulator.EPA, RegulatorCategory.EPA)]
    [InlineData(Regulator.EPA_RCRA, RegulatorCategory.EPA)]
    [InlineData(Regulator.EPA_TSCA, RegulatorCategory.EPA)]
    [InlineData(Regulator.NFPA, RegulatorCategory.NFPA)]
    [InlineData(Regulator.NFPA_Dust, RegulatorCategory.NFPA)]
    [InlineData(Regulator.PHMSA, RegulatorCategory.PHMSA)]
    [InlineData(Regulator.FDA, RegulatorCategory.FDA)]
    [InlineData(Regulator.ATF, RegulatorCategory.ATF)]
    [InlineData(Regulator.DoD, RegulatorCategory.ATF)]
    [InlineData(Regulator.MSHA, RegulatorCategory.MSHA)]
    [InlineData(Regulator.USCG, RegulatorCategory.USCG)]
    [InlineData(Regulator.DHS_CFATS, RegulatorCategory.DHS_CFATS)]
    public void RegulatorCategoryCatalog_ForRegulator_MatchesDocumentedFolding(Regulator regulator, RegulatorCategory expected)
    {
        Assert.Equal(expected, RegulatorCategoryCatalog.ForRegulator(regulator));
    }

    [Fact]
    public void RegulatorCategoryCatalog_ForRegulator_IsTotalOverEveryRegulator()
    {
        foreach (Regulator regulator in Enum.GetValues<Regulator>())
        {
            RegulatorCategoryCatalog.ForRegulator(regulator);
        }
    }

    [Fact]
    public void RegulatorRequirementCatalog_All_CoversEveryRegulator()
    {
        foreach (Regulator regulator in Enum.GetValues<Regulator>())
        {
            Assert.NotEmpty(RegulatorRequirementCatalog.ForRegulator(regulator));
        }
    }

    [Fact]
    public void RegulatorRequirementCatalog_EveryEntry_CategoryMatchesRegulatorCategoryCatalog()
    {
        foreach (RegulatorRequirement requirement in RegulatorRequirementCatalog.All)
        {
            Assert.Equal(RegulatorCategoryCatalog.ForRegulator(requirement.Regulator), requirement.Category);
        }
    }

    [Fact]
    public void RegulatorRequirementCatalog_ForCategory_PartitionsAllRequirementsExactlyOnce()
    {
        int totalAcrossCategories = Enum.GetValues<RegulatorCategory>()
            .Sum(category => RegulatorRequirementCatalog.ForCategory(category).Count);

        Assert.Equal(RegulatorRequirementCatalog.All.Count, totalAcrossCategories);
    }

    [Fact]
    public void RegulatorRequirementCatalog_HasNoDuplicateRequirementNames()
    {
        var distinctNames = RegulatorRequirementCatalog.All.Select(requirement => requirement.Name).Distinct(StringComparer.Ordinal).Count();

        Assert.Equal(RegulatorRequirementCatalog.All.Count, distinctNames);
    }

    [Fact]
    public void RegulatorRequirementMapping_For_EqualsUnionOfForRegulatorAcrossIndustryRegulators()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            int expected = industry.Regulators.Sum(regulator => RegulatorRequirementCatalog.ForRegulator(regulator).Count);

            Assert.Equal(expected, RegulatorRequirementMapping.For(industry).Count);
            Assert.Equal(RegulatorRequirementMapping.For(industry).Count, RegulatorRequirementMapping.ByIndustryName(industry.Name).Count);
        }
    }

    [Fact]
    public void RegulatorRequirementMapping_ByIndustryName_UnknownName_ReturnsEmpty()
    {
        Assert.Empty(RegulatorRequirementMapping.ByIndustryName("Not A Real Industry"));
    }

    [Fact]
    public void RegulatorSafetyModuleMapping_ByRegulator_IsTotalOverEveryRegulator()
    {
        foreach (Regulator regulator in Enum.GetValues<Regulator>())
        {
            Assert.True(RegulatorSafetyModuleMapping.ByRegulator.ContainsKey(regulator));
            RegulatorSafetyModuleMapping.ForRegulator(regulator);
        }
    }

    [Fact]
    public void RegulatorSopRequirements_ForRegulator_HasOneEntryPerCatalogRequirement()
    {
        foreach (Regulator regulator in Enum.GetValues<Regulator>())
        {
            Assert.Equal(RegulatorRequirementCatalog.ForRegulator(regulator).Count, RegulatorSopRequirements.ForRegulator(regulator).Count);
        }
    }

    [Fact]
    public void RegulatorPermitRequirements_ForRegulator_ReturnsSingleCanonicalPermitType()
    {
        foreach (Regulator regulator in Enum.GetValues<Regulator>())
        {
            Assert.Single(RegulatorPermitRequirements.ForRegulator(regulator));
            Assert.Equal(PermitTypeCatalog.ForRegulator(regulator), RegulatorPermitRequirements.ForRegulator(regulator)[0]);
        }
    }

    [Fact]
    public void RegulatorAuditRequirements_ForRegulator_ReturnsSingleCanonicalChecklistItem()
    {
        foreach (Regulator regulator in Enum.GetValues<Regulator>())
        {
            Assert.Single(RegulatorAuditRequirements.ForRegulator(regulator));
            Assert.Equal(AuditChecklistItemCatalog.ForRegulator(regulator), RegulatorAuditRequirements.ForRegulator(regulator)[0]);
        }
    }

    [Fact]
    public void RegulatorTrainingRequirements_ForRegulator_HasOneEntryPerCatalogRequirement()
    {
        foreach (Regulator regulator in Enum.GetValues<Regulator>())
        {
            Assert.Equal(RegulatorRequirementCatalog.ForRegulator(regulator).Count, RegulatorTrainingRequirements.ForRegulator(regulator).Count);
        }
    }
}
