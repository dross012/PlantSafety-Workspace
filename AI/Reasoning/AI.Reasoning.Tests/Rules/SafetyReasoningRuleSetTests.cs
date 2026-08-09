// ============================================================================
//  Project:        AI.Reasoning.Tests
//  File:           SafetyReasoningRuleSetTests.cs
//  Created:        2026-08-09
//  Description:    Tests for SafetyReasoningRuleSet.Standard, evaluated
//                  through the real SafetyReasoningEngine.Reason.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Lifecycle;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;

using Enterprise.WorkflowOrchestration.Abstractions.States;

using AI.Reasoning.Abstractions.Context;
using AI.Reasoning.Abstractions.Engine;
using AI.Reasoning.Abstractions.Rules;

using Xunit;

namespace AI.Reasoning.Tests.Rules;

public sealed class SafetyReasoningRuleSetTests
{
    private static SafetyReasoningContext CreateContext(
        FacilityLifecycleStatus status = FacilityLifecycleStatus.Active,
        int employeeInjuryCount = 0,
        IReadOnlyList<RegulatoryJurisdiction>? jurisdictions = null,
        SeverityLevel severity = SeverityLevel.Minor,
        EnterpriseWorkflowState workflowState = EnterpriseWorkflowState.InProgress,
        SeverityLevel twinSeverity = SeverityLevel.Minor) =>
        new(
            Guid.NewGuid(),
            new SafetyReasoningInput(
                status,
                employeeInjuryCount,
                0,
                HazardType.Fire,
                severity,
                jurisdictions ?? [RegulatoryJurisdiction.OSHA],
                workflowState,
                EnterpriseGraphConnectedFacilityCount: 0,
                DigitalTwinSimulatedSeverity: twinSeverity),
            DateTime.UtcNow);

    [Fact]
    public void Reason_ActiveFacilityWithInjury_ContributesIsolationNote()
    {
        SafetyReasoningContext context = CreateContext(employeeInjuryCount: 1);

        SafetyReasoningOutput output = SafetyReasoningEngine.Reason(context, SafetyReasoningRuleSet.Standard);

        output.Conclusions.Should().Contain(c => c.Contains("isolated"));
    }

    [Fact]
    public void Reason_MultipleJurisdictions_ContributesCoordinationNote()
    {
        SafetyReasoningContext context = CreateContext(jurisdictions: [RegulatoryJurisdiction.OSHA, RegulatoryJurisdiction.EPA]);

        SafetyReasoningOutput output = SafetyReasoningEngine.Reason(context, SafetyReasoningRuleSet.Standard);

        output.Conclusions.Should().Contain(c => c.Contains("Multiple regulatory jurisdictions"));
    }

    [Fact]
    public void Reason_SevereSeverityWithPendingWorkflow_ContributesEscalationOverdueNote()
    {
        SafetyReasoningContext context = CreateContext(severity: SeverityLevel.Severe, workflowState: EnterpriseWorkflowState.Pending);

        SafetyReasoningOutput output = SafetyReasoningEngine.Reason(context, SafetyReasoningRuleSet.Standard);

        output.Conclusions.Should().Contain(c => c.Contains("escalation is overdue"));
    }

    [Fact]
    public void Reason_NoConditionsMet_NoConclusions()
    {
        SafetyReasoningContext context = CreateContext(status: FacilityLifecycleStatus.PendingActivation, jurisdictions: []);

        SafetyReasoningOutput output = SafetyReasoningEngine.Reason(context, SafetyReasoningRuleSet.Standard);

        output.Conclusions.Should().BeEmpty();
    }
}
