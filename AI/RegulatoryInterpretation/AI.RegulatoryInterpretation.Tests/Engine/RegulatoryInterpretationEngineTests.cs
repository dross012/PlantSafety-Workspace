// ============================================================================
//  Project:        AI.RegulatoryInterpretation.Tests
//  File:           RegulatoryInterpretationEngineTests.cs
//  Created:        2026-08-08
//  Description:    Phase E.4 unit tests for RegulatoryInterpretationEngine.Interpret:
//                  jurisdiction inference delegation, per-jurisdiction
//                  guidance/reporting-window presence, and no-jurisdiction
//                  handling.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;

using AI.RegulatoryInterpretation.Abstractions.Engine;
using AI.RegulatoryInterpretation.Abstractions.Interpretation;
using AI.RegulatoryInterpretation.Abstractions.Rules;

using Xunit;

namespace AI.RegulatoryInterpretation.Tests.Engine;

public sealed class RegulatoryInterpretationEngineTests
{
    [Fact]
    public void Interpret_SevereFire_IncludesOshaAndNfpa()
    {
        var input = new RegulatoryInterpretationInput(HazardType.Fire, SeverityLevel.Severe);

        RegulatoryInterpretationOutput output = RegulatoryInterpretationEngine.Interpret(input);

        output.Interpretations.Select(i => i.Jurisdiction).Should().Contain([RegulatoryJurisdiction.OSHA, RegulatoryJurisdiction.NFPA]);
    }

    [Fact]
    public void Interpret_MinorOtherHazard_ReturnsNoInterpretations()
    {
        var input = new RegulatoryInterpretationInput(HazardType.Other, SeverityLevel.Minor);

        RegulatoryInterpretationOutput output = RegulatoryInterpretationEngine.Interpret(input);

        output.Interpretations.Should().BeEmpty();
    }

    [Fact]
    public void Interpret_CatastrophicFire_IncludesStateAndLocalWithGuidance()
    {
        var input = new RegulatoryInterpretationInput(HazardType.Fire, SeverityLevel.Catastrophic);

        RegulatoryInterpretationOutput output = RegulatoryInterpretationEngine.Interpret(input);

        output.Interpretations.Should().Contain(i => i.Jurisdiction == RegulatoryJurisdiction.State && !string.IsNullOrWhiteSpace(i.Guidance));
        output.Interpretations.Should().Contain(i => i.Jurisdiction == RegulatoryJurisdiction.Local && !string.IsNullOrWhiteSpace(i.Guidance));
    }

    [Fact]
    public void RegulatoryInterpretationCatalog_EveryJurisdiction_HasPositiveReportingWindow()
    {
        foreach (RegulatoryJurisdiction jurisdiction in Enum.GetValues<RegulatoryJurisdiction>())
        {
            RegulatoryInterpretationCatalog.InterpretationFor(jurisdiction).ReportingWindowHours.Should().BePositive();
        }
    }
}
