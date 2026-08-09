// ============================================================================
//  Project:        AI.SafetyCopilot.Tests
//  File:           SafetyCopilotEngineTests.cs
//  Created:        2026-08-08
//  Description:    Phase E.8 unit tests for SafetyCopilotEngine.Guide,
//                  exercised end-to-end against real outputs of Path E's
//                  other four engines: employee vs. supervisor action
//                  routing, hazard explanation wording, severity summary
//                  wording, and regulatory guidance per audience.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Severity;

using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Abstractions.Triggers;

using AI.HazardUnderstanding.Abstractions.Engine;
using AI.HazardUnderstanding.Abstractions.Semantics;
using AI.RegulatoryInterpretation.Abstractions.Engine;
using AI.RegulatoryInterpretation.Abstractions.Interpretation;
using AI.SafetyCopilot.Abstractions.Copilot;
using AI.SafetyCopilot.Abstractions.Engine;
using AI.SeverityAssessment.Abstractions.Assessment;
using AI.SeverityAssessment.Abstractions.Engine;
using AI.WorkflowAutomation.Abstractions.Automation;
using AI.WorkflowAutomation.Abstractions.Engine;

using Xunit;

namespace AI.SafetyCopilot.Tests.Engine;

public sealed class SafetyCopilotEngineTests
{
    private static SafetyCopilotInput CreateInput(CopilotAudience audience)
    {
        HazardUnderstandingOutput hazardUnderstanding = HazardUnderstandingEngine.Understand(
            new HazardUnderstandingInput(HazardType.Fire, PropagationStage.Fire, EmployeeInjuryCount: 1, MachineFailureCount: 0, EventChainLength: 2));

        WorkflowAutomationOutput workflowAutomation = WorkflowAutomationEngine.Automate(
            new WorkflowAutomationInput(
                new WorkflowContext(HazardType.Fire, SeverityLevel.Severe, EvacuationScope.Building, EmployeeExposureCount: 0, MachineFailureCount: 0, EventChainLength: 2),
                RequiresShutdown: false,
                RequiresRestart: false),
            []);

        RegulatoryInterpretationOutput regulatoryInterpretation = RegulatoryInterpretationEngine.Interpret(
            new RegulatoryInterpretationInput(HazardType.Fire, SeverityLevel.Severe));

        SeverityAssessmentOutput severityAssessment = SeverityAssessmentEngine.Assess(
            new SeverityAssessmentInput(HazardType.Fire, EmployeeInjuryCount: 1, MachineFailureCount: 0, EventChainLength: 2, DigitalTwinSimulatedSeverity: SeverityLevel.Moderate),
            []);

        return new SafetyCopilotInput(audience, hazardUnderstanding, workflowAutomation, regulatoryInterpretation, severityAssessment);
    }

    [Fact]
    public void Guide_Employee_ActionsAreRequiredActionsAndEvacuation()
    {
        SafetyCopilotOutput output = SafetyCopilotEngine.Guide(CreateInput(CopilotAudience.Employee));

        output.Actions.Should().NotBeEmpty();
        output.Actions.Should().OnlyContain(a => a.Audience == CopilotAudience.Employee);
        output.Actions.Should().Contain(a => a.ActionCode == "EVACUATE");
    }

    [Fact]
    public void Guide_Supervisor_ActionsAreSopAndPermit()
    {
        SafetyCopilotOutput output = SafetyCopilotEngine.Guide(CreateInput(CopilotAudience.Supervisor));

        output.Actions.Should().OnlyContain(a => a.Audience == CopilotAudience.Supervisor);
        output.Actions.Should().Contain(a => a.ActionCode == "FIRE-RESPONSE-SOP");
    }

    [Fact]
    public void Guide_IncludesHazardExplanationForBothAudiences()
    {
        SafetyCopilotOutput employeeOutput = SafetyCopilotEngine.Guide(CreateInput(CopilotAudience.Employee));
        SafetyCopilotOutput supervisorOutput = SafetyCopilotEngine.Guide(CreateInput(CopilotAudience.Supervisor));

        employeeOutput.Recommendations.Should().Contain(r => r.Title == "Hazard Explanation");
        supervisorOutput.Recommendations.Should().Contain(r => r.Title == "Hazard Explanation");
    }

    [Fact]
    public void Guide_SupervisorHazardExplanation_IncludesRelatedHazardsAndPropagation()
    {
        SafetyCopilotOutput output = SafetyCopilotEngine.Guide(CreateInput(CopilotAudience.Supervisor));

        output.Recommendations.Single(r => r.Title == "Hazard Explanation").Guidance.Should().Contain("Propagation:");
    }

    [Fact]
    public void Guide_SupervisorRegulatoryGuidance_OneRecommendationPerJurisdiction()
    {
        SafetyCopilotInput input = CreateInput(CopilotAudience.Supervisor);

        SafetyCopilotOutput output = SafetyCopilotEngine.Guide(input);

        int regulatoryRecommendationCount = output.Recommendations.Count(r => r.Title.StartsWith("Regulatory Guidance", StringComparison.Ordinal));
        regulatoryRecommendationCount.Should().Be(input.RegulatoryInterpretation.Interpretations.Count);
    }

    [Fact]
    public void Guide_EmployeeRegulatoryGuidance_IsSingleSimplifiedRecommendation()
    {
        SafetyCopilotOutput output = SafetyCopilotEngine.Guide(CreateInput(CopilotAudience.Employee));

        output.Recommendations.Count(r => r.Title.StartsWith("Regulatory Guidance", StringComparison.Ordinal)).Should().Be(1);
    }

    [Fact]
    public void Guide_SeveritySummary_ReflectsFinalAssessedSeverity()
    {
        SafetyCopilotOutput output = SafetyCopilotEngine.Guide(CreateInput(CopilotAudience.Employee));

        output.Recommendations.Single(r => r.Title == "Severity Assessment").Guidance.Should().Contain("Severe");
    }
}
