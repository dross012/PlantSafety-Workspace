// ============================================================================
//  Project:        AI.WorkflowAutomation.Tests
//  File:           WorkflowAutomationRuleSetTests.cs
//  Created:        2026-08-09
//  Description:    Tests for WorkflowAutomationRuleSet.Standard, evaluated
//                  through the real WorkflowAutomationEngine.Automate.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Severity;

using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Abstractions.Triggers;

using Enterprise.WorkflowOrchestration.Abstractions.Steps;

using AI.WorkflowAutomation.Abstractions.Automation;
using AI.WorkflowAutomation.Abstractions.Engine;
using AI.WorkflowAutomation.Abstractions.Rules;

using Xunit;

namespace AI.WorkflowAutomation.Tests.Rules;

public sealed class WorkflowAutomationRuleSetTests
{
    private static WorkflowAutomationInput CreateInput(SeverityLevel severity = SeverityLevel.Minor, int machineFailureCount = 0, bool requiresShutdown = false, bool requiresRestart = false) =>
        new(
            new WorkflowContext(HazardType.Fire, severity, EvacuationScope.Building, EmployeeExposureCount: 0, MachineFailureCount: machineFailureCount, EventChainLength: 1),
            requiresShutdown,
            requiresRestart);

    [Fact]
    public void Automate_RequestedShutdown_GeneratesRequestedShutdownStep()
    {
        WorkflowAutomationOutput output = WorkflowAutomationEngine.Automate(CreateInput(requiresShutdown: true), WorkflowAutomationRuleSet.Standard);

        output.ShutdownRestartSequence.Should().Contain(s => s.Code == "SHUTDOWN-REQUESTED");
    }

    [Fact]
    public void Automate_CatastrophicSeverityWithoutRequestedShutdown_ForcesShutdown()
    {
        WorkflowAutomationOutput output = WorkflowAutomationEngine.Automate(CreateInput(severity: SeverityLevel.Catastrophic), WorkflowAutomationRuleSet.Standard);

        output.ShutdownRestartSequence.Should().Contain(s => s.Code == "SHUTDOWN-CATASTROPHIC");
    }

    [Fact]
    public void Automate_TwoMachineFailuresWithoutRequestedShutdown_ForcesShutdown()
    {
        WorkflowAutomationOutput output = WorkflowAutomationEngine.Automate(CreateInput(machineFailureCount: 2), WorkflowAutomationRuleSet.Standard);

        output.ShutdownRestartSequence.Should().Contain(s => s.Code == "SHUTDOWN-MACHINE-FAILURES");
    }

    [Fact]
    public void Automate_MinorSeverityNoFailuresNoRequest_NoShutdownGenerated()
    {
        WorkflowAutomationOutput output = WorkflowAutomationEngine.Automate(CreateInput(), WorkflowAutomationRuleSet.Standard);

        output.ShutdownRestartSequence.Should().BeEmpty();
    }
}
