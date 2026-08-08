// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           FacilityProfileTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Facility Profile subsystem:
//                  FacilityProfileBuilder, FacilityComplianceProfileBuilder,
//                  FacilityDashboardMetadataBuilder,
//                  FacilityEventRoutingRulesBuilder, and
//                  FacilityOnboardingTemplateBuilder. Verifies determinism,
//                  totality across all 40 industries (no exceptions), and
//                  internal self-consistency (e.g. required-element counts
//                  match across the mapping and compliance layers).
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class FacilityProfileTests
{
    [Fact]
    public void Build_UnknownIndustry_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Not A Real Industry", "Nowhere"));
    }

    [Fact]
    public void Build_RequiredCountsMatchProgramRequirementMapping()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), industry.Name, industry.Name, "Loc");
            IReadOnlyList<ProgramDefinition> expectedPrograms = ProgramRequirementMapping.For(industry);

            Assert.Equal(expectedPrograms.Count, facility.RequiredPrograms.Count);
            Assert.Equal(expectedPrograms.Count, facility.RequiredTraining.Count);
            Assert.Equal(expectedPrograms.Count, facility.RequiredAudits.Count);
            Assert.Equal(expectedPrograms.Count, facility.RequiredSops.Count);
            Assert.Equal(industry.Flags, facility.Flags);
            Assert.Equal(industry.Regulators, facility.Regulators);
        }
    }

    [Fact]
    public void Build_ComplianceProfile_RequiredElementsCountEqualsSumOfKinds()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Houston, TX");
        FacilityComplianceProfile profile = facility.ComplianceProfile;

        int expected = facility.RequiredPrograms.Count
            + facility.RequiredPermits.Count
            + facility.RequiredTraining.Count
            + facility.RequiredAudits.Count
            + facility.RequiredSops.Count;

        Assert.Equal(expected, profile.RequiredElements.Count);
    }

    [Fact]
    public void Build_NoCompletedElements_ScoreIsZeroAndAllMissing()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Medical Devices", "Loc");

        Assert.Equal(0d, facility.ComplianceProfile.ComplianceScorePercent);
        Assert.Empty(facility.ComplianceProfile.CompletedElements);
        Assert.Equal(facility.ComplianceProfile.RequiredElements.Count, facility.ComplianceProfile.MissingElements.Count);
    }

    [Fact]
    public void Build_AllElementsCompleted_ScoreIsHundredAndRiskIsLow()
    {
        FacilityProfile draft = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Medical Devices", "Loc");
        var allCodes = draft.ComplianceProfile.RequiredElements.Select(e => e.Code).ToHashSet();

        FacilityProfile facility = FacilityProfileBuilder.Build(draft.FacilityId, "X", "Medical Devices", "Loc", allCodes);

        Assert.Equal(100d, facility.ComplianceProfile.ComplianceScorePercent);
        Assert.Empty(facility.ComplianceProfile.MissingElements);
        Assert.Equal(ComplianceRiskLevel.Low, facility.ComplianceProfile.RiskLevel);
        Assert.Empty(facility.ComplianceProfile.PriorityActions);
    }

    [Fact]
    public void Build_PriorityActions_NeverExceedsTen()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");

        Assert.True(facility.ComplianceProfile.PriorityActions.Count <= 10);
    }

    [Fact]
    public void DashboardMetadataBuilder_AlertCountMatchesMissingProgramCount()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        FacilityDashboardMetadata dashboard = FacilityDashboardMetadataBuilder.Build(facility);

        int missingPrograms = facility.ComplianceProfile.MissingElements.Count(e => e.Kind == ComplianceElementKind.Program);

        Assert.Equal(missingPrograms, dashboard.RequiredAlerts.Count);
        Assert.Equal(facility.FacilityId, dashboard.FacilityId);
    }

    [Fact]
    public void DashboardMetadataBuilder_RequiredWidgets_IncludeOneWidgetPerHazardFlag()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        FacilityDashboardMetadata dashboard = FacilityDashboardMetadataBuilder.Build(facility);

        int expectedFlagWidgets = SafetyFlagCatalog.Decompose(facility.Flags).Count;

        Assert.Equal(expectedFlagWidgets + 2, dashboard.RequiredWidgets.Count);
    }

    [Fact]
    public void EventRoutingRulesBuilder_OneRoutePerRequiredProgram()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        IReadOnlyList<FacilityEventRoute> routes = FacilityEventRoutingRulesBuilder.Build(facility);

        Assert.Equal(facility.RequiredPrograms.Count, routes.Count);
        foreach (FacilityEventRoute route in routes)
        {
            Assert.Contains(facility.RequiredPrograms, program => program.Code == route.ProgramCode);
        }
    }

    [Fact]
    public void OnboardingTemplateBuilder_ByIndustryName_HasEntryForEveryIndustry()
    {
        Assert.Equal(IndustryRegistry.All.Count, FacilityOnboardingTemplateBuilder.ByIndustryName.Count);

        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            FacilityOnboardingTemplate template = FacilityOnboardingTemplateBuilder.ByIndustryName[industry.Name];
            IReadOnlyList<ProgramDefinition> expectedPrograms = ProgramRequirementMapping.For(industry);

            Assert.Equal(expectedPrograms.Count, template.ProgramActivationList.Count);
            Assert.Equal(expectedPrograms.Select(p => p.Code), template.ProgramActivationList.Select(p => p.Code));
        }
    }

    [Fact]
    public void AllBuilders_NeverThrow_ForEveryIndustry()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), industry.Name, industry.Name, "Loc");
            FacilityDashboardMetadataBuilder.Build(facility);
            FacilityEventRoutingRulesBuilder.Build(facility);
            FacilityOnboardingTemplateBuilder.For(industry);
        }
    }
}
