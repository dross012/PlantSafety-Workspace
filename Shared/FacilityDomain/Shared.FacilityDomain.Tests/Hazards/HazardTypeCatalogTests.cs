// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           HazardTypeCatalogTests.cs
//  Created:        2026-08-08
//  Description:    Phase B.2 unit tests for HazardMetadataCatalog and
//                  HazardInferenceRules.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;

using Xunit;

namespace Shared.FacilityDomain.Tests.Hazards;

public sealed class HazardTypeCatalogTests
{
    [Fact]
    public void HazardMetadataCatalog_HasExactlyOneEntryPerHazardType()
    {
        HazardMetadataCatalog.All.Should().HaveCount(Enum.GetValues<HazardType>().Length);
        HazardMetadataCatalog.All.Select(m => m.HazardType).Should().BeEquivalentTo(Enum.GetValues<HazardType>());
    }

    [Theory]
    [InlineData(HazardType.Fire, HazardCategory.PhysicalSafety)]
    [InlineData(HazardType.Injury, HazardCategory.PhysicalSafety)]
    [InlineData(HazardType.ConfinedSpace, HazardCategory.PhysicalSafety)]
    [InlineData(HazardType.HotWork, HazardCategory.PhysicalSafety)]
    [InlineData(HazardType.LOTOFailure, HazardCategory.OperationalProcess)]
    [InlineData(HazardType.ChemicalSpill, HazardCategory.ChemicalEnvironmental)]
    [InlineData(HazardType.EnvironmentalRelease, HazardCategory.ChemicalEnvironmental)]
    [InlineData(HazardType.FleetIncident, HazardCategory.Transportation)]
    [InlineData(HazardType.Other, HazardCategory.Uncategorized)]
    public void HazardCategoryCatalog_ForHazardType_MapsAsDocumented(HazardType hazardType, HazardCategory expected)
    {
        HazardCategoryCatalog.ForHazardType(hazardType).Should().Be(expected);
    }

    [Theory]
    [InlineData("AccidentManagement.FireEvent", null, HazardType.Fire)]
    [InlineData("Accident.Created", "chemical spill in tank farm", HazardType.ChemicalSpill)]
    [InlineData("SafetyEvent.Occurred", "worker injury on line 3", HazardType.Injury)]
    [InlineData("Crisis.Declared", "environmental release detected", HazardType.EnvironmentalRelease)]
    [InlineData("Emergency.Triggered", "confined space entry incident", HazardType.ConfinedSpace)]
    [InlineData("Accident.Created", "lockout tagout procedure bypassed", HazardType.LOTOFailure)]
    [InlineData("Accident.Created", "hot work permit violation", HazardType.HotWork)]
    [InlineData("Accident.Created", "fleet vehicle collision", HazardType.FleetIncident)]
    [InlineData("Accident.Created", "unspecified incident", HazardType.Other)]
    public void InferHazardTypeFromAccident_ClassifiesByKeyword(string eventType, string? description, HazardType expected)
    {
        HazardInferenceRules.InferHazardTypeFromAccident(eventType, description).Should().Be(expected);
    }

    [Fact]
    public void InferHazardTypeFromSafetyEvent_CrisisEvent_EmergencyEvent_AllUseSameClassification()
    {
        HazardInferenceRules.InferHazardTypeFromSafetyEvent("SafetyEvent", "fire alarm activated").Should().Be(HazardType.Fire);
        HazardInferenceRules.InferHazardTypeFromCrisisEvent("Crisis", "fire alarm activated").Should().Be(HazardType.Fire);
        HazardInferenceRules.InferHazardTypeFromEmergencyEvent("Emergency", "fire alarm activated").Should().Be(HazardType.Fire);
    }
}
