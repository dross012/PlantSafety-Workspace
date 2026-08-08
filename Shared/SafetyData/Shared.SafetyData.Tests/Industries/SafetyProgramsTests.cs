// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           SafetyProgramsTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Safety Programs subsystem:
//                  ProgramCatalog, ProgramRequirementMapping, and the six
//                  Program-first consumer mappings (ProgramSopRequirements,
//                  ProgramTrainingRequirements, ProgramPermitRequirements,
//                  ProgramAuditRequirements, ProgramEventRoutingRules,
//                  ProgramComplianceProfiles). Verifies ProgramCatalog is
//                  built 1:1 from RegulatorRequirementCatalog, that exactly
//                  the three FDA programs have a null PrimaryFlag, and that
//                  every derived artifact stays consistent with the program
//                  it was built from.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class SafetyProgramsTests
{
    [Fact]
    public void ProgramCatalog_All_IsBuiltOneToOneFromRegulatorRequirementCatalog()
    {
        Assert.Equal(RegulatorRequirementCatalog.All.Count, ProgramCatalog.All.Count);
    }

    [Fact]
    public void ProgramCatalog_All_HasUniqueCodes()
    {
        var distinctCodes = ProgramCatalog.All.Select(program => program.Code).Distinct(StringComparer.Ordinal).Count();

        Assert.Equal(ProgramCatalog.All.Count, distinctCodes);
    }

    [Fact]
    public void ProgramCatalog_All_EveryProgramNameMatchesASourceRequirement()
    {
        var requirementNames = RegulatorRequirementCatalog.All.Select(requirement => requirement.Name).ToHashSet(StringComparer.Ordinal);

        foreach (ProgramDefinition program in ProgramCatalog.All)
        {
            Assert.Contains(program.Name, requirementNames);
            Assert.Equal(RegulatorCategoryCatalog.ForRegulator(program.PrimaryRegulator), program.Category);
        }
    }

    [Fact]
    public void ProgramCatalog_ExactlyThreeProgramsHaveNullPrimaryFlag_AllFromFda()
    {
        var nullFlagPrograms = ProgramCatalog.All.Where(program => program.PrimaryFlag is null).ToArray();

        Assert.Equal(3, nullFlagPrograms.Length);
        Assert.All(nullFlagPrograms, program => Assert.Equal(Regulator.FDA, program.PrimaryRegulator));
    }

    [Fact]
    public void ProgramCatalog_ForRegulator_PartitionsAllProgramsExactlyOnce()
    {
        int totalAcrossRegulators = Enum.GetValues<Regulator>().Sum(regulator => ProgramCatalog.ForRegulator(regulator).Count);

        Assert.Equal(ProgramCatalog.All.Count, totalAcrossRegulators);
    }

    [Fact]
    public void ProgramCatalog_FindByCode_RoundTripsForEveryProgram()
    {
        foreach (ProgramDefinition program in ProgramCatalog.All)
        {
            Assert.Same(program, ProgramCatalog.FindByCode(program.Code));
        }
    }

    [Fact]
    public void ProgramCatalog_FindByCode_UnknownCode_ReturnsNull()
    {
        Assert.Null(ProgramCatalog.FindByCode("PROG-DOES-NOT-EXIST"));
    }

    [Fact]
    public void ProgramRequirementMapping_AppliesTo_NeverThrows_ForEveryProgramAndIndustryCombination()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            foreach (ProgramDefinition program in ProgramCatalog.All)
            {
                ProgramRequirementMapping.AppliesTo(program, industry);
            }
        }
    }

    [Fact]
    public void ProgramRequirementMapping_For_AllAppliedProgramsSatisfyAppliesTo()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            foreach (ProgramDefinition program in ProgramRequirementMapping.For(industry))
            {
                Assert.True(ProgramRequirementMapping.AppliesTo(program, industry));
            }
        }
    }

    [Fact]
    public void ProgramRequirementMapping_ChemicalManufacturing_IncludesLotoProgramViaFlag()
    {
        IndustryDefinition chemicalManufacturing = IndustryRegistry.All.Single(industry => industry.Name == "Chemical Manufacturing");
        ProgramDefinition lotoProgram = ProgramCatalog.All.Single(program => program.Name == "Lockout/Tagout (LOTO)");

        Assert.Contains(lotoProgram, ProgramRequirementMapping.For(chemicalManufacturing));
    }

    [Fact]
    public void ProgramRequirementMapping_MedicalDevices_ExcludesAtfPrograms()
    {
        IndustryDefinition medicalDevices = IndustryRegistry.All.Single(industry => industry.Name == "Medical Devices");

        IReadOnlyList<ProgramDefinition> applicable = ProgramRequirementMapping.For(medicalDevices);

        Assert.DoesNotContain(applicable, program => program.PrimaryRegulator == Regulator.ATF);
    }

    [Fact]
    public void ProgramRequirementMapping_ByIndustryName_UnknownName_ReturnsEmpty()
    {
        Assert.Empty(ProgramRequirementMapping.ByIndustryName("Not A Real Industry"));
    }

    [Fact]
    public void ProgramSopRequirements_ByProgramCode_HasOneEntryPerProgram()
    {
        Assert.Equal(ProgramCatalog.All.Count, ProgramSopRequirements.ByProgramCode.Count);

        foreach (ProgramDefinition program in ProgramCatalog.All)
        {
            Assert.Equal($"SOP-{program.Code}", ProgramSopRequirements.For(program).Code);
        }
    }

    [Fact]
    public void ProgramTrainingRequirements_ByProgramCode_HasOneEntryPerProgram()
    {
        Assert.Equal(ProgramCatalog.All.Count, ProgramTrainingRequirements.ByProgramCode.Count);

        foreach (ProgramDefinition program in ProgramCatalog.All)
        {
            Assert.Equal($"TRN-{program.Code}", ProgramTrainingRequirements.For(program).Code);
        }
    }

    [Fact]
    public void ProgramPermitRequirements_For_MatchesPermitTypeCatalogForRegulator()
    {
        foreach (ProgramDefinition program in ProgramCatalog.All)
        {
            Assert.Equal(PermitTypeCatalog.ForRegulator(program.PrimaryRegulator), ProgramPermitRequirements.For(program));
        }
    }

    [Fact]
    public void ProgramAuditRequirements_ByProgramCode_HasOneEntryPerProgram()
    {
        Assert.Equal(ProgramCatalog.All.Count, ProgramAuditRequirements.ByProgramCode.Count);

        foreach (ProgramDefinition program in ProgramCatalog.All)
        {
            Assert.Equal($"AUDIT-{program.Code}", ProgramAuditRequirements.For(program).Code);
        }
    }

    [Fact]
    public void ProgramEventRoutingRules_TopicFor_UsesFlagTopicWhenFlagPresentOtherwiseGeneratedTopic()
    {
        foreach (ProgramDefinition program in ProgramCatalog.All)
        {
            string topic = ProgramEventRoutingRules.TopicFor(program);

            if (program.PrimaryFlag is SafetyFlag flag)
            {
                Assert.Equal(EventRoutingTopicCatalog.TopicForFlag(flag), topic);
            }
            else
            {
                Assert.Equal($"safety.events.program.{program.Code.ToLowerInvariant()}", topic);
            }
        }
    }

    [Fact]
    public void ProgramComplianceProfiles_All_AggregatesEveryOtherConsumerConsistently()
    {
        Assert.Equal(ProgramCatalog.All.Count, ProgramComplianceProfiles.All.Count);

        foreach (ProgramDefinition program in ProgramCatalog.All)
        {
            ProgramComplianceProfile profile = ProgramComplianceProfiles.ByProgramCode[program.Code];

            Assert.Equal(ProgramSopRequirements.For(program), profile.Sop);
            Assert.Equal(ProgramTrainingRequirements.For(program), profile.Training);
            Assert.Equal(ProgramPermitRequirements.For(program), profile.Permit);
            Assert.Equal(ProgramAuditRequirements.For(program), profile.AuditItem);
            Assert.Equal(ProgramEventRoutingRules.TopicFor(program), profile.EventRoutingTopic);

            foreach (string industryName in profile.ApplicableIndustryNames)
            {
                IndustryDefinition industry = IndustryRegistry.All.Single(i => i.Name == industryName);
                Assert.True(ProgramRequirementMapping.AppliesTo(program, industry));
            }
        }
    }

    [Fact]
    public void ProgramComplianceProfiles_FindByProgramCode_UnknownCode_ReturnsNull()
    {
        Assert.Null(ProgramComplianceProfiles.FindByProgramCode("PROG-DOES-NOT-EXIST"));
    }
}
