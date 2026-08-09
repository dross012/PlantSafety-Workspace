// ============================================================================
//  Project:        AI.SeverityAssessment.Tests
//  File:           SeverityAssessmentEngineTests.cs
//  Created:        2026-08-08
//  Description:    Phase E.5 unit tests for SeverityAssessmentEngine.Assess:
//                  base severity delegation, digital-twin worst-case
//                  comparison, and rule-contributed notes.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

using AI.SeverityAssessment.Abstractions.Assessment;
using AI.SeverityAssessment.Abstractions.Engine;
using AI.SeverityAssessment.Abstractions.Rules;

using Xunit;

namespace AI.SeverityAssessment.Tests.Engine;

public sealed class SeverityAssessmentEngineTests
{
    [Fact]
    public void Assess_MultipleInjuries_BaseSeverityIsCatastrophic()
    {
        var input = new SeverityAssessmentInput(HazardType.Other, EmployeeInjuryCount: 3, MachineFailureCount: 0, EventChainLength: 1, DigitalTwinSimulatedSeverity: SeverityLevel.Minor);

        SeverityAssessmentOutput output = SeverityAssessmentEngine.Assess(input, []);

        output.BaseSeverity.Should().Be(SeverityLevel.Catastrophic);
        output.FinalAssessedSeverity.Should().Be(SeverityLevel.Catastrophic);
    }

    [Fact]
    public void Assess_TwinSimulationWorseThanBase_FinalReflectsTwin()
    {
        var input = new SeverityAssessmentInput(HazardType.Other, EmployeeInjuryCount: 0, MachineFailureCount: 0, EventChainLength: 1, DigitalTwinSimulatedSeverity: SeverityLevel.Catastrophic);

        SeverityAssessmentOutput output = SeverityAssessmentEngine.Assess(input, []);

        output.BaseSeverity.Should().Be(SeverityLevel.Minor);
        output.FinalAssessedSeverity.Should().Be(SeverityLevel.Catastrophic);
    }

    [Fact]
    public void Assess_MatchedRule_ContributesNote()
    {
        var input = new SeverityAssessmentInput(HazardType.Fire, EmployeeInjuryCount: 0, MachineFailureCount: 0, EventChainLength: 1, DigitalTwinSimulatedSeverity: SeverityLevel.Minor);
        var rules = new[]
        {
            new SeverityAssessmentRule("R1", "Fire note", (i, baseSeverity) => i.HazardType == HazardType.Fire, "Fire hazard detected."),
        };

        SeverityAssessmentOutput output = SeverityAssessmentEngine.Assess(input, rules);

        output.Notes.Should().ContainSingle().Which.Should().Be("Fire hazard detected.");
    }

    [Fact]
    public void Assess_UnmatchedRule_DoesNotContributeNote()
    {
        var input = new SeverityAssessmentInput(HazardType.Other, EmployeeInjuryCount: 0, MachineFailureCount: 0, EventChainLength: 1, DigitalTwinSimulatedSeverity: SeverityLevel.Minor);
        var rules = new[]
        {
            new SeverityAssessmentRule("R1", "Fire note", (i, baseSeverity) => i.HazardType == HazardType.Fire, "Fire hazard detected."),
        };

        SeverityAssessmentOutput output = SeverityAssessmentEngine.Assess(input, rules);

        output.Notes.Should().BeEmpty();
    }
}
