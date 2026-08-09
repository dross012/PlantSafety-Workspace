// ============================================================================
//  Project:        AI.RegulatoryInterpretation.Tests
//  File:           RegulatoryInterpretationRuleSetTests.cs
//  Created:        2026-08-09
//  Description:    Tests for RegulatoryInterpretationRuleSet.Standard's
//                  escalation notes, evaluated through the real
//                  RegulatoryInterpretationEngine.Interpret.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

using AI.RegulatoryInterpretation.Abstractions.Engine;
using AI.RegulatoryInterpretation.Abstractions.Interpretation;

using Xunit;

namespace AI.RegulatoryInterpretation.Tests.Rules;

public sealed class RegulatoryInterpretationRuleSetTests
{
    [Fact]
    public void Interpret_CatastrophicFire_ContributesMultiJurisdictionNote()
    {
        var input = new RegulatoryInterpretationInput(HazardType.Fire, SeverityLevel.Catastrophic);

        RegulatoryInterpretationOutput output = RegulatoryInterpretationEngine.Interpret(input);

        output.EscalationNotes.Should().Contain(n => n.Contains("Three or more jurisdictions"));
    }

    [Fact]
    public void Interpret_CatastrophicFire_ContributesFederalAndStateNote()
    {
        var input = new RegulatoryInterpretationInput(HazardType.Fire, SeverityLevel.Catastrophic);

        RegulatoryInterpretationOutput output = RegulatoryInterpretationEngine.Interpret(input);

        output.EscalationNotes.Should().Contain(n => n.Contains("federal (OSHA) and state"));
    }

    [Fact]
    public void Interpret_CatastrophicFire_ContributesCatastrophicReportingNote()
    {
        var input = new RegulatoryInterpretationInput(HazardType.Fire, SeverityLevel.Catastrophic);

        RegulatoryInterpretationOutput output = RegulatoryInterpretationEngine.Interpret(input);

        output.EscalationNotes.Should().Contain(n => n.Contains("shortest applicable reporting window"));
    }

    [Fact]
    public void Interpret_MinorOtherHazard_NoEscalationNotes()
    {
        var input = new RegulatoryInterpretationInput(HazardType.Other, SeverityLevel.Minor);

        RegulatoryInterpretationOutput output = RegulatoryInterpretationEngine.Interpret(input);

        output.EscalationNotes.Should().BeEmpty();
    }
}
