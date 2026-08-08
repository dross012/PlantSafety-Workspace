// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           FacilityEventingComplianceDependencyInjectionTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for AddFacilityProfiles(), AddEventing(), and
//                  AddComplianceRisk() DI registration -- each extension
//                  registered and resolved in isolation, mirroring
//                  IndustryDependencyInjectionTests.cs's shape for
//                  AddIndustries/AddRegulatoryRequirements/AddSafetyPrograms.
//                  Cross-subsystem composition (all six registered together,
//                  as Reporting.Infrastructure actually does) is covered by
//                  Shared.SafetyData.IntegrationTests, not here.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;
using Shared.SafetyData.Infrastructure.DependencyInjection;
using Shared.SafetyData.Infrastructure.Industries;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class FacilityEventingComplianceDependencyInjectionTests
{
    [Fact]
    public void AddFacilityProfiles_RegistersFacilityProfileServiceAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddFacilityProfiles();
        using ServiceProvider provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<IFacilityProfileService>();
        Assert.IsType<FacilityProfileService>(service);
        Assert.Same(service, provider.GetRequiredService<IFacilityProfileService>());

        FacilityProfile facility = service.BuildProfile(Guid.NewGuid(), "X", "Medical Devices", "Loc");
        Assert.Equal("Medical Devices", facility.IndustryName);
    }

    [Fact]
    public void AddEventing_RegistersAllThreeServicesAsSingletons()
    {
        var services = new ServiceCollection();
        services.AddFacilityProfiles();
        services.AddEventing();
        using ServiceProvider provider = services.BuildServiceProvider();

        var routing = provider.GetRequiredService<IEventRoutingService>();
        var impact = provider.GetRequiredService<IEventImpactService>();
        var workflow = provider.GetRequiredService<IEventWorkflowService>();

        Assert.IsType<EventRoutingService>(routing);
        Assert.IsType<EventImpactService>(impact);
        Assert.IsType<EventWorkflowService>(workflow);
        Assert.Same(routing, provider.GetRequiredService<IEventRoutingService>());

        var facilityService = provider.GetRequiredService<IFacilityProfileService>();
        FacilityProfile facility = facilityService.BuildProfile(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");

        EventRoutingResult result = routing.Route("ChemicalSpill", facility);
        Assert.Equal("ChemicalSpill", result.EventCode);

        EventImpactProfile impactProfile = impact.ComputeImpact("ChemicalSpill", facility);
        Assert.Equal("ChemicalSpill", impactProfile.EventCode);

        EventWorkflowTemplate? template = workflow.GetWorkflow("ChemicalSpill");
        Assert.NotNull(template);
        Assert.Equal(5, template!.Steps.Count);
    }

    [Fact]
    public void EventRoutingService_UnknownEventCode_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        services.AddFacilityProfiles();
        services.AddEventing();
        using ServiceProvider provider = services.BuildServiceProvider();

        var facilityService = provider.GetRequiredService<IFacilityProfileService>();
        FacilityProfile facility = facilityService.BuildProfile(Guid.NewGuid(), "X", "Medical Devices", "Loc");

        var routing = provider.GetRequiredService<IEventRoutingService>();
        Assert.Throws<ArgumentException>(() => routing.Route("NotARealEvent", facility));
    }

    [Fact]
    public void EventWorkflowService_UnknownEventCode_ReturnsNull()
    {
        var services = new ServiceCollection();
        services.AddEventing();
        using ServiceProvider provider = services.BuildServiceProvider();

        var workflow = provider.GetRequiredService<IEventWorkflowService>();
        Assert.Null(workflow.GetWorkflow("NotARealEvent"));
    }

    [Fact]
    public void AddComplianceRisk_RegistersComplianceRiskServiceAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddFacilityProfiles();
        services.AddComplianceRisk();
        using ServiceProvider provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<IComplianceRiskService>();
        Assert.IsType<ComplianceRiskService>(service);
        Assert.Same(service, provider.GetRequiredService<IComplianceRiskService>());

        var facilityService = provider.GetRequiredService<IFacilityProfileService>();
        FacilityProfile facility = facilityService.BuildProfile(Guid.NewGuid(), "X", "Medical Devices", "Loc");

        ComplianceScore score = service.ComputeComplianceScore(facility);
        Assert.Equal(facility.FacilityId, score.FacilityId);

        RiskScore risk = service.ComputeRiskScore(facility);
        Assert.Equal(facility.FacilityId, risk.FacilityId);

        (ComplianceReadiness complianceReadiness, RiskReadiness riskReadiness) = service.ComputeReadiness(facility);
        Assert.Equal(facility.FacilityId, complianceReadiness.FacilityId);
        Assert.Equal(facility.FacilityId, riskReadiness.FacilityId);
    }

    [Fact]
    public void AllSixSubsystems_RegisterTogetherWithoutConflict()
    {
        var services = new ServiceCollection();
        services.AddIndustries();
        services.AddRegulatoryRequirements();
        services.AddSafetyPrograms();
        services.AddFacilityProfiles();
        services.AddEventing();
        services.AddComplianceRisk();
        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<Abstractions.Models.Industries.IIndustryRegistry>());
        Assert.NotNull(provider.GetRequiredService<Abstractions.Models.Industries.IRegulatoryRequirementRegistry>());
        Assert.NotNull(provider.GetRequiredService<Abstractions.Models.Industries.IProgramRegistry>());
        Assert.NotNull(provider.GetRequiredService<IFacilityProfileService>());
        Assert.NotNull(provider.GetRequiredService<IEventRoutingService>());
        Assert.NotNull(provider.GetRequiredService<IEventImpactService>());
        Assert.NotNull(provider.GetRequiredService<IEventWorkflowService>());
        Assert.NotNull(provider.GetRequiredService<IComplianceRiskService>());
    }
}
