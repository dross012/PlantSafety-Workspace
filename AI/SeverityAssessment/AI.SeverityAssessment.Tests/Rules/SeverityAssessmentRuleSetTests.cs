// ============================================================================
//  Project:        AI.SeverityAssessment.Tests
//  File:           SeverityAssessmentRuleSetTests.cs
//  Created:        2026-08-09
//  Description:    Tests for SeverityAssessmentRuleSet.Standard, evaluated
//                  through the real SeverityAssessmentEngine.Assess.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

using AI.SeverityAssessment.Abstractions.Assessment;
using AI.SeverityAssessment.Abstractions.Engine;
using AI.SeverityAssessment.Abstractions.Rules;

using Xunit;

namespace AI.SeverityAssessment.Tests.Rules;

public sealed class SeverityAssessmentRuleSetTests
{
    [Fact]
    public void Assess_MultipleInjuriesAndFailures_ContributesBothNotes()
    {
        var input = new SeverityAssessmentInput(HazardType.Other, EmployeeInjuryCount: 2, MachineFailureCount: 2, EventChainLength: 1, DigitalTwinSimulatedSeverity: SeverityLevel.Minor);

        SeverityAssessmentOutput output = SeverityAssessmentEngine.Assess(input, SeverityAssessmentRuleSet.Standard);

        output.Notes.Should().Contain(n => n.Contains("Multiple employee injuries"));
        output.Notes.Should().Contain(n => n.Contains("cascading equipment failure"));
    }

    [Fact]
    public void Assess_FireWithInjury_ContributesFireNote()
    {
        var input = new SeverityAssessmentInput(HazardType.Fire, EmployeeInjuryCount: 1, MachineFailureCount: 0, EventChainLength: 1, DigitalTwinSimulatedSeverity: SeverityLevel.Minor);

        SeverityAssessmentOutput output = SeverityAssessmentEngine.Assess(input, SeverityAssessmentRuleSet.Standard);

        output.Notes.Should().Contain(n => n.Contains("evacuation and fire-suppression"));
    }

    [Fact]
    public void Assess_NoRuleConditionsMet_NoNotes()
    {
        var input = new SeverityAssessmentInput(HazardType.Other, EmployeeInjuryCount: 0, MachineFailureCount: 0, EventChainLength: 0, DigitalTwinSimulatedSeverity: SeverityLevel.Minor);

        SeverityAssessmentOutput output = SeverityAssessmentEngine.Assess(input, SeverityAssessmentRuleSet.Standard);

        output.Notes.Should().BeEmpty();
    }
}
