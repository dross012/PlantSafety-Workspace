// ============================================================================
//  Project:        AI.Reasoning.Tests
//  File:           SafetyReasoningEngineTests.cs
//  Created:        2026-08-08
//  Description:    Phase E.1 unit tests for SafetyReasoningEngine.Reason:
//                  matched rules contribute conclusions, unmatched rules
//                  don't, and OverallAssessedSeverity is the worst case
//                  across the input's severity signals.
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

namespace AI.Reasoning.Tests.Engine;

public sealed class SafetyReasoningEngineTests
{
    private static SafetyReasoningContext CreateContext(
        SeverityLevel severity = SeverityLevel.Minor,
        SeverityLevel twinSeverity = SeverityLevel.Minor,
        int employeeInjuryCount = 0,
        int machineFailureCount = 0) =>
        new(
            Guid.NewGuid(),
            new SafetyReasoningInput(
                FacilityLifecycleStatus.Active,
                employeeInjuryCount,
                machineFailureCount,
                HazardType.Fire,
                severity,
                [RegulatoryJurisdiction.OSHA],
                EnterpriseWorkflowState.InProgress,
                EnterpriseGraphConnectedFacilityCount: 2,
                DigitalTwinSimulatedSeverity: twinSeverity),
            DateTime.UtcNow);

    [Fact]
    public void Reason_MatchedRule_ContributesConclusion()
    {
        SafetyReasoningContext context = CreateContext(employeeInjuryCount: 2);
        var rules = new[]
        {
            new SafetyReasoningRule("R1", "Injury check", ctx => ctx.Input.EmployeeInjuryCount > 0, "Employee injuries detected."),
        };

        SafetyReasoningOutput output = SafetyReasoningEngine.Reason(context, rules);

        output.Conclusions.Should().ContainSingle().Which.Should().Be("Employee injuries detected.");
    }

    [Fact]
    public void Reason_UnmatchedRule_DoesNotContribute()
    {
        SafetyReasoningContext context = CreateContext(employeeInjuryCount: 0);
        var rules = new[]
        {
            new SafetyReasoningRule("R1", "Injury check", ctx => ctx.Input.EmployeeInjuryCount > 0, "Employee injuries detected."),
        };

        SafetyReasoningOutput output = SafetyReasoningEngine.Reason(context, rules);

        output.Conclusions.Should().BeEmpty();
    }

    [Fact]
    public void Reason_MultipleRules_EvaluatesEachIndependently()
    {
        SafetyReasoningContext context = CreateContext(employeeInjuryCount: 1, machineFailureCount: 1);
        var rules = new[]
        {
            new SafetyReasoningRule("R1", "Injury", ctx => ctx.Input.EmployeeInjuryCount > 0, "Injury conclusion"),
            new SafetyReasoningRule("R2", "Failure", ctx => ctx.Input.MachineFailureCount > 0, "Failure conclusion"),
            new SafetyReasoningRule("R3", "Never matches", ctx => ctx.Input.MachineFailureCount > 100, "Should not appear"),
        };

        SafetyReasoningOutput output = SafetyReasoningEngine.Reason(context, rules);

        output.Conclusions.Should().BeEquivalentTo(["Injury conclusion", "Failure conclusion"]);
    }

    [Fact]
    public void Reason_OverallSeverity_IsWorstCaseAcrossSeverityAndTwinSignals()
    {
        SafetyReasoningContext context = CreateContext(severity: SeverityLevel.Moderate, twinSeverity: SeverityLevel.Catastrophic);

        SafetyReasoningOutput output = SafetyReasoningEngine.Reason(context, []);

        output.OverallAssessedSeverity.Should().Be(SeverityLevel.Catastrophic);
    }

    [Fact]
    public void Reason_OverallSeverity_PrefersInputSeverityWhenHigherThanTwin()
    {
        SafetyReasoningContext context = CreateContext(severity: SeverityLevel.Severe, twinSeverity: SeverityLevel.Minor);

        SafetyReasoningOutput output = SafetyReasoningEngine.Reason(context, []);

        output.OverallAssessedSeverity.Should().Be(SeverityLevel.Severe);
    }

    [Fact]
    public void Reason_PreservesReasoningSessionIdAndTimestamp()
    {
        SafetyReasoningContext context = CreateContext();

        SafetyReasoningOutput output = SafetyReasoningEngine.Reason(context, []);

        output.ReasoningSessionId.Should().Be(context.ReasoningSessionId);
        output.GeneratedAtUtc.Should().Be(context.EvaluatedAtUtc);
    }
}
