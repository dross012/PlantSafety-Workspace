// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           IndustryConsumerArtifactsTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the eight Industry subsystem consumer
//                  artifacts under Models/Industries/Consumers/: HazardMapping,
//                  RequiredSafetyModules, PermitRequirements, SopsRegistry,
//                  AuditChecklistRegistry, DashboardMetadataRegistry,
//                  EventRoutingRules, and ComplianceProfiles. Each is derived
//                  from IndustryRegistry.All, so these tests verify totality
//                  (no exceptions across all 40 industries), determinism
//                  (For(industry) matches ByIndustryName/All lookups), and
//                  cross-consistency between artifacts derived from the same
//                  underlying flags.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Abstractions.Models.Industries.Consumers;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class IndustryConsumerArtifactsTests
{
    [Fact]
    public void HazardMapping_For_MatchesFlagCountForEveryIndustry()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            Assert.Equal(SafetyFlagCatalog.Decompose(industry.Flags).Count, HazardMapping.For(industry).Count);
            Assert.Equal(HazardMapping.For(industry).Count, HazardMapping.ByIndustryName[industry.Name].Count);
        }
    }

    [Fact]
    public void RequiredSafetyModules_For_MatchesFlagCountForEveryIndustry()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            Assert.Equal(SafetyFlagCatalog.Decompose(industry.Flags).Count, RequiredSafetyModules.For(industry).Count);
        }
    }

    [Fact]
    public void PermitRequirements_For_IsSubsetOfAllPermitTypesAndNeverEmpty()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            IReadOnlyList<PermitType> permits = PermitRequirements.For(industry);

            Assert.NotEmpty(permits);
            Assert.True(permits.Count <= industry.Regulators.Count);
        }
    }

    [Fact]
    public void SopsRegistry_For_MatchesFlagCountForEveryIndustry()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            Assert.Equal(SafetyFlagCatalog.Decompose(industry.Flags).Count, SopsRegistry.For(industry).Count);
        }
    }

    [Fact]
    public void AuditChecklistRegistry_For_EqualsFlagItemsPlusRegulatorItems()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            int expected = SafetyFlagCatalog.Decompose(industry.Flags).Count + industry.Regulators.Count;

            Assert.Equal(expected, AuditChecklistRegistry.For(industry).Count);
        }
    }

    [Fact]
    public void DashboardMetadataRegistry_All_HasOneEntryPerIndustryWithConsistentCounts()
    {
        Assert.Equal(IndustryRegistry.All.Count, DashboardMetadataRegistry.All.Count);

        foreach (DashboardIndustryMetadata metadata in DashboardMetadataRegistry.All)
        {
            IndustryDefinition industry = IndustryRegistry.All.Single(i => i.Name == metadata.Name);

            Assert.Equal(SafetyFlagCatalog.Decompose(industry.Flags).Count, metadata.HazardFlagCount);
            Assert.Equal(industry.Regulators.Count, metadata.RegulatorCount);
        }
    }

    [Theory]
    [InlineData(4, RiskTier.Standard)]
    [InlineData(5, RiskTier.Standard)]
    [InlineData(6, RiskTier.Elevated)]
    [InlineData(7, RiskTier.Elevated)]
    [InlineData(8, RiskTier.Critical)]
    [InlineData(9, RiskTier.Critical)]
    public void DashboardMetadataRegistry_RiskTierFor_UsesDocumentedThresholds(int flagCount, RiskTier expectedTier)
    {
        SafetyFlag flags = SafetyFlagCatalog.AllFlags.Take(flagCount).Aggregate(SafetyFlag.None, (accumulated, flag) => accumulated | flag);

        Assert.Equal(expectedTier, DashboardMetadataRegistry.RiskTierFor(flags));
    }

    [Fact]
    public void EventRoutingRules_All_HasOneRulePerFlagAndCoversEveryIndustry()
    {
        Assert.Equal(SafetyFlagCatalog.AllFlags.Count, EventRoutingRules.All.Count);

        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            Assert.Equal(SafetyFlagCatalog.Decompose(industry.Flags).Count, EventRoutingRules.TopicsFor(industry).Count);

            foreach (string topic in EventRoutingRules.TopicsFor(industry))
            {
                EventRoutingRule rule = EventRoutingRules.All.Single(r => r.Topic == topic);
                Assert.Contains(industry.Name, rule.IndustryNames);
            }
        }
    }

    [Fact]
    public void ComplianceProfiles_All_AggregatesEveryOtherConsumerConsistently()
    {
        Assert.Equal(IndustryRegistry.All.Count, ComplianceProfiles.All.Count);

        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            ComplianceProfile profile = ComplianceProfiles.ByIndustryName[industry.Name];

            Assert.Equal(industry.TopLevelCategory, profile.TopLevelCategory);
            Assert.Equal(HazardMapping.For(industry).Count, profile.Hazards.Count);
            Assert.Equal(RequiredSafetyModules.For(industry).Count, profile.RequiredModules.Count);
            Assert.Equal(PermitRequirements.For(industry).Count, profile.RequiredPermits.Count);
            Assert.Equal(SopsRegistry.For(industry).Count, profile.SopTemplates.Count);
            Assert.Equal(AuditChecklistRegistry.For(industry).Count, profile.AuditChecklist.Count);
            Assert.Equal(EventRoutingRules.TopicsFor(industry).Count, profile.EventRoutingTopics.Count);
        }
    }

    [Fact]
    public void ComplianceProfiles_FindByIndustryName_UnknownName_ReturnsNull()
    {
        Assert.Null(ComplianceProfiles.FindByIndustryName("Not A Real Industry"));
    }
}
