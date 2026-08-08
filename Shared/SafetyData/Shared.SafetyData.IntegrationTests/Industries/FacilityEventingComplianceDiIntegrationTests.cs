// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         IntegrationTests.Industries
//  Layer:          Tests (Integration)
//  File:           FacilityEventingComplianceDiIntegrationTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Integration tests for the full six-subsystem composition,
//                  as actually wired together in a real host (see
//                  Reporting.Infrastructure's InfrastructureServiceCollectionExtensions.
//                  AddReportingInfrastructure, which calls AddIndustries(),
//                  AddRegulatoryRequirements(), AddSafetyPrograms(),
//                  AddFacilityProfiles(), AddEventing(), and
//                  AddComplianceRisk() in that order). Builds a real
//                  IServiceCollection/IServiceProvider and drives the full
//                  chain end to end: resolve IIndustryRegistry -> build a
//                  FacilityProfile via IFacilityProfileService -> interpret
//                  an event via IEventRoutingService/IEventImpactService ->
//                  score/trend it via IComplianceRiskService -- verifying
//                  every service agrees with the others and with their own
//                  static catalogs when composed together.
// ============================================================================
using Microsoft.Extensions.DependencyInjection;

using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Abstractions.Models.Industries.ComplianceRisk;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;
using Shared.SafetyData.Infrastructure.DependencyInjection;

using Xunit;

namespace Shared.SafetyData.IntegrationTests.Industries;

public sealed class FacilityEventingComplianceDiIntegrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddIndustries();
        services.AddRegulatoryRequirements();
        services.AddSafetyPrograms();
        services.AddFacilityProfiles();
        services.AddEventing();
        services.AddComplianceRisk();
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AllSixSubsystems_ResolveAsSingletons()
    {
        using ServiceProvider provider = BuildProvider();

        Assert.Same(provider.GetRequiredService<IIndustryRegistry>(), provider.GetRequiredService<IIndustryRegistry>());
        Assert.Same(provider.GetRequiredService<IRegulatoryRequirementRegistry>(), provider.GetRequiredService<IRegulatoryRequirementRegistry>());
        Assert.Same(provider.GetRequiredService<IProgramRegistry>(), provider.GetRequiredService<IProgramRegistry>());
        Assert.Same(provider.GetRequiredService<IFacilityProfileService>(), provider.GetRequiredService<IFacilityProfileService>());
        Assert.Same(provider.GetRequiredService<IEventRoutingService>(), provider.GetRequiredService<IEventRoutingService>());
        Assert.Same(provider.GetRequiredService<IEventImpactService>(), provider.GetRequiredService<IEventImpactService>());
        Assert.Same(provider.GetRequiredService<IEventWorkflowService>(), provider.GetRequiredService<IEventWorkflowService>());
        Assert.Same(provider.GetRequiredService<IComplianceRiskService>(), provider.GetRequiredService<IComplianceRiskService>());
    }

    [Theory]
    [InlineData("Chemical Manufacturing", "ChemicalSpill")]
    [InlineData("Medical Devices", "Injury")]
    [InlineData("Marine Terminals", "UscgWaterfrontIncident")]
    [InlineData("Cannabis", "OshaRecordable")]
    public void FullChain_IndustryToFacilityToEventToComplianceRisk_ProducesConsistentResults(string industryName, string eventCode)
    {
        using ServiceProvider provider = BuildProvider();

        var industryRegistry = provider.GetRequiredService<IIndustryRegistry>();
        var facilityService = provider.GetRequiredService<IFacilityProfileService>();
        var routingService = provider.GetRequiredService<IEventRoutingService>();
        var impactService = provider.GetRequiredService<IEventImpactService>();
        var workflowService = provider.GetRequiredService<IEventWorkflowService>();
        var complianceRiskService = provider.GetRequiredService<IComplianceRiskService>();

        IndustryDefinition? industry = industryRegistry.FindByName(industryName);
        Assert.NotNull(industry);

        FacilityProfile facility = facilityService.BuildProfile(Guid.NewGuid(), $"{industryName} Test Facility", industryName, "Test Location");
        Assert.Equal(industry!.Name, facility.IndustryName);
        Assert.Equal(industry.Flags, facility.Flags);

        EventRoutingResult routing = routingService.Route(eventCode, facility);
        EventImpactProfile impact = impactService.ComputeImpact(eventCode, facility);
        Assert.Equal(routing.EventCode, impact.EventCode);
        Assert.Equal(facility.FacilityId, routing.FacilityId);
        Assert.Equal(facility.FacilityId, impact.FacilityId);

        ComplianceScore complianceScore = complianceRiskService.ComputeComplianceScore(facility);
        RiskScore riskScore = complianceRiskService.ComputeRiskScore(facility);
        Assert.Equal(facility.FacilityId, complianceScore.FacilityId);
        Assert.Equal(facility.FacilityId, riskScore.FacilityId);
        Assert.Equal(facility.ComplianceProfile.ComplianceScorePercent, complianceScore.PercentComplete);

        var complianceDashboard = complianceRiskService.ComputeComplianceDashboard(facility);
        var riskDashboard = complianceRiskService.ComputeRiskDashboard(facility);
        Assert.NotEmpty(complianceDashboard.RequiredWidgets);
        Assert.NotEmpty(riskDashboard.RequiredWidgets);

        (ComplianceReadiness complianceReadiness, RiskReadiness riskReadiness) = complianceRiskService.ComputeReadiness(facility);
        Assert.Equal(facility.FacilityId, complianceReadiness.FacilityId);
        Assert.Equal(facility.FacilityId, riskReadiness.FacilityId);

        // Workflow is optional -- only 10 of the 21 events have one.
        EventWorkflowTemplate? workflow = workflowService.GetWorkflow(eventCode);
        if (workflow is not null)
        {
            Assert.Equal(5, workflow.Steps.Count);
        }
    }

    [Fact]
    public void ComplianceTrend_ImprovesAsElementsAreCompleted_ThroughFullServiceChain()
    {
        using ServiceProvider provider = BuildProvider();
        var facilityService = provider.GetRequiredService<IFacilityProfileService>();
        var complianceRiskService = provider.GetRequiredService<IComplianceRiskService>();

        Guid facilityId = Guid.NewGuid();
        FacilityProfile before = facilityService.BuildProfile(facilityId, "X", "Medical Devices", "Loc");
        var allCodes = before.ComplianceProfile.RequiredElements.Select(e => e.Code).ToHashSet();
        FacilityProfile after = facilityService.BuildProfile(facilityId, "X", "Medical Devices", "Loc", allCodes);

        ComplianceScore beforeScore = complianceRiskService.ComputeComplianceScore(before);
        ComplianceScore afterScore = complianceRiskService.ComputeComplianceScore(after);

        ComplianceTrend trend = complianceRiskService.ComputeComplianceTrend(beforeScore, afterScore);

        Assert.Equal(ComplianceTrendDirection.Improving, trend.Direction);
        Assert.Equal(100d, trend.Delta);
    }

    [Fact]
    public void AllServices_NeverThrow_AcrossEveryIndustryAndEventCombination()
    {
        using ServiceProvider provider = BuildProvider();
        var industryRegistry = provider.GetRequiredService<IIndustryRegistry>();
        var facilityService = provider.GetRequiredService<IFacilityProfileService>();
        var routingService = provider.GetRequiredService<IEventRoutingService>();
        var impactService = provider.GetRequiredService<IEventImpactService>();
        var complianceRiskService = provider.GetRequiredService<IComplianceRiskService>();

        foreach (IndustryDefinition industry in industryRegistry.All)
        {
            FacilityProfile facility = facilityService.BuildProfile(Guid.NewGuid(), industry.Name, industry.Name, "Loc");
            complianceRiskService.ComputeComplianceScore(facility);
            complianceRiskService.ComputeRiskScore(facility);
            complianceRiskService.ComputeComplianceDashboard(facility);
            complianceRiskService.ComputeRiskDashboard(facility);
            complianceRiskService.ComputeReadiness(facility);

            foreach (EventDefinition eventDefinition in EventCatalog.All)
            {
                routingService.Route(eventDefinition.EventCode, facility);
                impactService.ComputeImpact(eventDefinition.EventCode, facility);
            }
        }
    }
}
