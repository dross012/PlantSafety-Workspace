// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         IntegrationTests.Industries
//  Layer:          Tests (Integration)
//  File:           IndustryTaxonomyDiIntegrationTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the full Industry -> Regulatory
//                  Requirements -> Safety Programs composition, as actually
//                  wired together in a real host (see Reporting.Infrastructure's
//                  InfrastructureServiceCollectionExtensions.AddReportingInfrastructure,
//                  which calls AddIndustries(), AddRegulatoryRequirements(),
//                  and AddSafetyPrograms() in that order). Unlike
//                  Shared.SafetyData.Tests (unit tests against the static
//                  catalogs directly), these tests build a real
//                  IServiceCollection/IServiceProvider, resolve
//                  IIndustryRegistry/IRegulatoryRequirementRegistry/
//                  IProgramRegistry through DI, and verify the three
//                  subsystems agree with each other and with their own
//                  static catalogs when composed together -- catching wiring
//                  regressions (wrong lifetime, wrong implementation,
//                  missing registration) that unit tests against the static
//                  types alone cannot.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Infrastructure.DependencyInjection;

using Xunit;

namespace Shared.SafetyData.IntegrationTests.Industries;

public sealed class IndustryTaxonomyDiIntegrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddIndustries();
        services.AddRegulatoryRequirements();
        services.AddSafetyPrograms();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddIndustries_AddRegulatoryRequirements_AddSafetyPrograms_AllResolveAsSingletons()
    {
        using ServiceProvider provider = BuildProvider();

        var industryRegistry1 = provider.GetRequiredService<IIndustryRegistry>();
        var industryRegistry2 = provider.GetRequiredService<IIndustryRegistry>();
        var regulatoryRegistry1 = provider.GetRequiredService<IRegulatoryRequirementRegistry>();
        var regulatoryRegistry2 = provider.GetRequiredService<IRegulatoryRequirementRegistry>();
        var programRegistry1 = provider.GetRequiredService<IProgramRegistry>();
        var programRegistry2 = provider.GetRequiredService<IProgramRegistry>();

        Assert.Same(industryRegistry1, industryRegistry2);
        Assert.Same(regulatoryRegistry1, regulatoryRegistry2);
        Assert.Same(programRegistry1, programRegistry2);
    }

    [Fact]
    public void ResolvedIndustryRegistry_MatchesStaticIndustryRegistry()
    {
        using ServiceProvider provider = BuildProvider();
        var industryRegistry = provider.GetRequiredService<IIndustryRegistry>();

        Assert.Equal(IndustryRegistry.All.Count, industryRegistry.All.Count);
        Assert.Equal(40, industryRegistry.All.Count);

        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            Assert.Same(industry, industryRegistry.FindByName(industry.Name));
        }
    }

    [Fact]
    public void ResolvedRegulatoryRegistry_MatchesStaticRegulatorRequirementCatalog()
    {
        using ServiceProvider provider = BuildProvider();
        var regulatoryRegistry = provider.GetRequiredService<IRegulatoryRequirementRegistry>();

        Assert.Equal(RegulatorRequirementCatalog.All.Count, regulatoryRegistry.All.Count);

        foreach (Regulator regulator in Enum.GetValues<Regulator>())
        {
            Assert.Equal(
                RegulatorRequirementCatalog.ForRegulator(regulator).Count,
                regulatoryRegistry.ForRegulator(regulator).Count);
            Assert.Equal(RegulatorCategoryCatalog.ForRegulator(regulator), regulatoryRegistry.CategoryFor(regulator));
        }
    }

    [Fact]
    public void ResolvedProgramRegistry_MatchesStaticProgramCatalog()
    {
        using ServiceProvider provider = BuildProvider();
        var programRegistry = provider.GetRequiredService<IProgramRegistry>();

        Assert.Equal(ProgramCatalog.All.Count, programRegistry.All.Count);

        foreach (ProgramDefinition program in ProgramCatalog.All)
        {
            Assert.Same(program, programRegistry.FindByCode(program.Code));
        }
    }

    [Theory]
    [InlineData("Chemical Manufacturing")]
    [InlineData("Medical Devices")]
    [InlineData("Marine Terminals")]
    [InlineData("Cannabis")]
    public void ResolvedRegistries_AgreeAcrossSubsystems_ForKnownIndustry(string industryName)
    {
        using ServiceProvider provider = BuildProvider();
        var industryRegistry = provider.GetRequiredService<IIndustryRegistry>();
        var regulatoryRegistry = provider.GetRequiredService<IRegulatoryRequirementRegistry>();
        var programRegistry = provider.GetRequiredService<IProgramRegistry>();

        IndustryDefinition? industry = industryRegistry.FindByName(industryName);
        Assert.NotNull(industry);

        IReadOnlyList<RegulatorRequirement> requirementsByIndustry = regulatoryRegistry.ForIndustry(industry!);
        IReadOnlyList<RegulatorRequirement> requirementsByName = regulatoryRegistry.ForIndustryName(industryName);
        Assert.Equal(requirementsByIndustry.Count, requirementsByName.Count);

        IReadOnlyList<ProgramDefinition> programsByIndustry = programRegistry.ForIndustry(industry!);
        IReadOnlyList<ProgramDefinition> programsByName = programRegistry.ForIndustryName(industryName);
        Assert.Equal(programsByIndustry.Count, programsByName.Count);

        // Every regulator on the industry must contribute at least one regulatory requirement.
        foreach (Regulator regulator in industry!.Regulators)
        {
            Assert.Contains(requirementsByIndustry, requirement => requirement.Regulator == regulator);
        }

        // Every applicable program's own requirement must be present in the industry's own
        // regulatory requirement list whenever the program applies via its regulator.
        foreach (ProgramDefinition program in programsByIndustry)
        {
            bool viaRegulator = requirementsByIndustry.Any(requirement =>
                requirement.Regulator == program.PrimaryRegulator && requirement.Name == program.Name);
            bool viaFlag = program.PrimaryFlag is SafetyFlag flag && industry.Flags.HasFlag(flag);

            Assert.True(viaRegulator || viaFlag);
        }
    }

    [Fact]
    public void ResolvedRegistries_NeverThrow_ForEveryIndustryInTheTaxonomy()
    {
        using ServiceProvider provider = BuildProvider();
        var industryRegistry = provider.GetRequiredService<IIndustryRegistry>();
        var regulatoryRegistry = provider.GetRequiredService<IRegulatoryRequirementRegistry>();
        var programRegistry = provider.GetRequiredService<IProgramRegistry>();

        foreach (IndustryDefinition industry in industryRegistry.All)
        {
            IReadOnlyList<RegulatorRequirement> requirements = regulatoryRegistry.ForIndustry(industry);
            IReadOnlyList<ProgramDefinition> programs = programRegistry.ForIndustry(industry);

            Assert.NotNull(requirements);
            Assert.NotNull(programs);
        }
    }

    [Fact]
    public void ResolvedProgramRegistry_ForIndustryName_UnknownName_ReturnsEmpty()
    {
        using ServiceProvider provider = BuildProvider();
        var programRegistry = provider.GetRequiredService<IProgramRegistry>();

        IReadOnlyList<ProgramDefinition> programs = programRegistry.ForIndustryName("Not A Real Industry");

        Assert.Empty(programs);
    }
}
