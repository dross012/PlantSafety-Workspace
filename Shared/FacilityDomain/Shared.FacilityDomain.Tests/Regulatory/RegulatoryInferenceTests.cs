// ============================================================================
//  Project:        Shared.FacilityDomain.Tests
//  File:           RegulatoryInferenceTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.4 unit tests for RegulatoryInferenceRules and
//                  RegulatoryMetadata: deterministic jurisdiction inference
//                  from hazard type and severity level.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;

using Xunit;

namespace Shared.FacilityDomain.Tests.Regulatory;

public sealed class RegulatoryInferenceTests
{
    [Fact]
    public void Infer_MinorOtherHazard_ReturnsNoJurisdictions()
    {
        IReadOnlyList<RegulatoryJurisdiction> result = RegulatoryInferenceRules.Infer(HazardType.Other, SeverityLevel.Minor);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Infer_ModerateOrWorse_AlwaysIncludesOsha()
    {
        IReadOnlyList<RegulatoryJurisdiction> result = RegulatoryInferenceRules.Infer(HazardType.Other, SeverityLevel.Moderate);

        result.Should().Contain(RegulatoryJurisdiction.OSHA);
    }

    [Fact]
    public void Infer_Fire_IncludesNfpa()
    {
        IReadOnlyList<RegulatoryJurisdiction> result = RegulatoryInferenceRules.Infer(HazardType.Fire, SeverityLevel.Severe);

        result.Should().Contain(RegulatoryJurisdiction.NFPA);
    }

    [Theory]
    [InlineData(HazardType.ChemicalSpill)]
    [InlineData(HazardType.EnvironmentalRelease)]
    public void Infer_EnvironmentalHazards_IncludesEpa(HazardType hazardType)
    {
        IReadOnlyList<RegulatoryJurisdiction> result = RegulatoryInferenceRules.Infer(hazardType, SeverityLevel.Severe);

        result.Should().Contain(RegulatoryJurisdiction.EPA);
    }

    [Fact]
    public void Infer_FleetIncident_IncludesDot()
    {
        IReadOnlyList<RegulatoryJurisdiction> result = RegulatoryInferenceRules.Infer(HazardType.FleetIncident, SeverityLevel.Severe);

        result.Should().Contain(RegulatoryJurisdiction.DOT);
    }

    [Fact]
    public void Infer_Catastrophic_IncludesStateAndLocal()
    {
        IReadOnlyList<RegulatoryJurisdiction> result = RegulatoryInferenceRules.Infer(HazardType.Fire, SeverityLevel.Catastrophic);

        result.Should().Contain(RegulatoryJurisdiction.State);
        result.Should().Contain(RegulatoryJurisdiction.Local);
    }

    [Fact]
    public void RegulatoryMetadata_CodeOf_ReturnsExpectedCodes()
    {
        RegulatoryMetadata.CodeOf(RegulatoryJurisdiction.OSHA).Should().Be("OSHA");
        RegulatoryMetadata.CodeOf(RegulatoryJurisdiction.EPA).Should().Be("EPA");
    }

    [Fact]
    public void RegulatoryMetadata_TypicalReportingWindowHours_ReturnsPositiveValueForEveryJurisdiction()
    {
        foreach (RegulatoryJurisdiction jurisdiction in Enum.GetValues<RegulatoryJurisdiction>())
        {
            RegulatoryMetadata.TypicalReportingWindowHours(jurisdiction).Should().BePositive();
        }
    }
}
