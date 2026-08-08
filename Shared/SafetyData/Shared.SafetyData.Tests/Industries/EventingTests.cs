// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Tests.Industries
//  Layer:          Tests (Unit)
//  File:           EventingTests.cs
//  Created:        2026-08-08
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Unit tests for the Eventing & Orchestration subsystem:
//                  EventCatalog, EventRoutingEngine, EventImpactProfileBuilder,
//                  and EventWorkflowTemplateCatalog. Verifies the catalog's
//                  size and derivation rules, that routing/impact never
//                  throws across every (event x industry) combination, and
//                  that every named workflow template has the documented
//                  5-step shape.
// ============================================================================
using Shared.SafetyData.Abstractions.Models.Industries;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyData.Abstractions.Models.Industries.Facilities;

using Xunit;

namespace Shared.SafetyData.Tests.Industries;

public sealed class EventingTests
{
    [Fact]
    public void EventCatalog_ContainsExactlyTwentyOneEvents()
    {
        Assert.Equal(21, EventCatalog.All.Count);
    }

    [Fact]
    public void EventCatalog_HasUniqueEventCodes()
    {
        var distinctCodes = EventCatalog.All.Select(e => e.EventCode).Distinct(StringComparer.Ordinal).Count();

        Assert.Equal(EventCatalog.All.Count, distinctCodes);
    }

    [Fact]
    public void EventCatalog_EveryEvent_HasExactlyOneRelatedRegulator()
    {
        foreach (EventDefinition eventDefinition in EventCatalog.All)
        {
            Assert.Single(eventDefinition.RelatedRegulators);
        }
    }

    [Theory]
    [InlineData("ChemicalSpill")]
    [InlineData("Fire")]
    [InlineData("ExplosionRisk")]
    [InlineData("ConfinedSpaceEntry")]
    [InlineData("LotoViolation")]
    [InlineData("DotHazmatIncident")]
    public void EventCatalog_FlaggedEvents_DeriveRelatedProgramsSopsTrainingPermitsFromPrimaryFlag(string eventCode)
    {
        EventDefinition eventDefinition = EventCatalog.FindByCode(eventCode)!;

        Assert.NotEmpty(eventDefinition.RelatedHazards);
        Assert.NotEmpty(eventDefinition.RelatedPrograms);
        Assert.Equal(eventDefinition.RelatedPrograms.Count, eventDefinition.RequiredSops.Count);
        Assert.Equal(eventDefinition.RelatedPrograms.Count, eventDefinition.RequiredTraining.Count);
    }

    [Theory]
    [InlineData("NearMiss")]
    [InlineData("Injury")]
    [InlineData("OshaRecordable")]
    [InlineData("RcraWasteIncident")]
    public void EventCatalog_UnflaggedEvents_HaveEmptyDerivedCollections(string eventCode)
    {
        EventDefinition eventDefinition = EventCatalog.FindByCode(eventCode)!;

        Assert.Empty(eventDefinition.RelatedHazards);
        Assert.Empty(eventDefinition.RelatedPrograms);
        Assert.Empty(eventDefinition.RequiredSops);
        Assert.Empty(eventDefinition.RequiredTraining);
        Assert.Empty(eventDefinition.RequiredPermits);
    }

    [Fact]
    public void EventCatalog_FindByCode_UnknownCode_ReturnsNull()
    {
        Assert.Null(EventCatalog.FindByCode("NotARealEvent"));
    }

    [Fact]
    public void EventCatalog_ForRegulator_PartitionsConsistentlyWithRelatedRegulators()
    {
        foreach (Regulator regulator in Enum.GetValues<Regulator>())
        {
            IReadOnlyList<EventDefinition> events = EventCatalog.ForRegulator(regulator);
            Assert.All(events, e => Assert.Contains(regulator, e.RelatedRegulators));
        }
    }

    [Fact]
    public void EventRoutingEngine_And_EventImpactProfileBuilder_NeverThrow_AcrossAllIndustriesAndEvents()
    {
        foreach (IndustryDefinition industry in IndustryRegistry.All)
        {
            FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), industry.Name, industry.Name, "Loc");

            foreach (EventDefinition eventDefinition in EventCatalog.All)
            {
                EventRoutingResult routing = EventRoutingEngine.Route(eventDefinition, facility);
                EventImpactProfile impact = EventImpactProfileBuilder.Build(eventDefinition, facility);

                Assert.Equal(eventDefinition.EventCode, routing.EventCode);
                Assert.Equal(eventDefinition.EventCode, impact.EventCode);
                Assert.Equal(facility.FacilityId, routing.FacilityId);
                Assert.Equal(facility.FacilityId, impact.FacilityId);
            }
        }
    }

    [Fact]
    public void EventRoutingEngine_ChemicalSpillAgainstChemicalManufacturing_ActivatesRelatedPrograms()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        EventDefinition chemicalSpill = EventCatalog.FindByCode("ChemicalSpill")!;

        EventRoutingResult result = EventRoutingEngine.Route(chemicalSpill, facility);

        Assert.Equal(chemicalSpill.RelatedPrograms.Count, result.ActivatedPrograms.Count);
        Assert.Equal(chemicalSpill.RequiredSops.Count, result.ActivatedSops.Count);
        Assert.NotEmpty(result.EventBusTopics);
    }

    [Fact]
    public void EventImpactProfileBuilder_CorrectiveActionsAreSubsetOfFacilityMissingElements()
    {
        FacilityProfile facility = FacilityProfileBuilder.Build(Guid.NewGuid(), "X", "Chemical Manufacturing", "Loc");
        EventDefinition chemicalSpill = EventCatalog.FindByCode("ChemicalSpill")!;

        EventImpactProfile impact = EventImpactProfileBuilder.Build(chemicalSpill, facility);

        Assert.All(impact.RequiredCorrectiveActions, element => Assert.Contains(element, facility.ComplianceProfile.MissingElements));
    }

    [Fact]
    public void EventWorkflowTemplateCatalog_HasTemplatesForExactlyTenEvents()
    {
        Assert.Equal(10, EventWorkflowTemplateCatalog.TemplatedEventCodes.Count);
        Assert.Equal(10, EventWorkflowTemplateCatalog.ByEventCode.Count);
    }

    [Fact]
    public void EventWorkflowTemplateCatalog_EveryTemplate_HasExactlyFiveOrderedSteps()
    {
        foreach (string eventCode in EventWorkflowTemplateCatalog.TemplatedEventCodes)
        {
            EventWorkflowTemplate template = EventWorkflowTemplateCatalog.ByEventCode[eventCode];

            Assert.Equal(5, template.Steps.Count);
            Assert.Equal(Enumerable.Range(1, 5), template.Steps.Select(s => s.Order));
        }
    }
}
