// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           IndustryDependencyInjectionTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for AddIndustries(), AddRegulatoryRequirements(),
//                  and AddSafetyPrograms() DI registration -- each extension
//                  registered and resolved in isolation, mirroring
//                  ServiceCollectionExtensionsTests.cs's shape for
//                  AddSharedSafetyData. Cross-subsystem composition (all
//                  three registered together, as Reporting.Infrastructure
//                  actually does) is covered by
//                  Shared.SafetyData.IntegrationTests, not here.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Infrastructure.DependencyInjection;
using Shared.SafetyData.Infrastructure.Industries;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class IndustryDependencyInjectionTests
{
    [Fact]
    public void AddIndustries_RegistersIndustryRegistryAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddIndustries();
        using ServiceProvider provider = services.BuildServiceProvider();

        var registry = provider.GetRequiredService<IIndustryRegistry>();
        Assert.IsType<IndustryRegistryProvider>(registry);
        Assert.Same(registry, provider.GetRequiredService<IIndustryRegistry>());
        Assert.Equal(IndustryRegistry.All.Count, registry.All.Count);
    }

    [Fact]
    public void AddRegulatoryRequirements_RegistersRegulatoryRequirementRegistryAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddRegulatoryRequirements();
        using ServiceProvider provider = services.BuildServiceProvider();

        var registry = provider.GetRequiredService<IRegulatoryRequirementRegistry>();
        Assert.IsType<RegulatoryRequirementRegistryProvider>(registry);
        Assert.Same(registry, provider.GetRequiredService<IRegulatoryRequirementRegistry>());
        Assert.Equal(RegulatorRequirementCatalog.All.Count, registry.All.Count);
    }

    [Fact]
    public void AddSafetyPrograms_RegistersProgramRegistryAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddSafetyPrograms();
        using ServiceProvider provider = services.BuildServiceProvider();

        var registry = provider.GetRequiredService<IProgramRegistry>();
        Assert.IsType<ProgramRegistryProvider>(registry);
        Assert.Same(registry, provider.GetRequiredService<IProgramRegistry>());
        Assert.Equal(ProgramCatalog.All.Count, registry.All.Count);
    }
}
