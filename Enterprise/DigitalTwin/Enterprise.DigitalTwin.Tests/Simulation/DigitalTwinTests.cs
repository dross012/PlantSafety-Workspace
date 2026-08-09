// ============================================================================
//  Project:        Enterprise.DigitalTwin.Tests
//  File:           DigitalTwinTests.cs
//  Created:        2026-08-08
//  Description:    Phase D.7 unit tests for DigitalTwinSimulationEngine:
//                  each of the five simulation kinds delegates correctly
//                  to the real Path C engines it composes, plus the
//                  facility-lifecycle simulation's own guard clauses.
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Lifecycle;
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Severity;

using Shared.FacilityDomain.Abstractions.Chains;

using Shared.WorkflowEngine.Abstractions.Engine;
using Shared.WorkflowEngine.Abstractions.Triggers;

using Enterprise.DigitalTwin.Abstractions.Simulation;

using Xunit;

namespace Enterprise.DigitalTwin.Tests.Simulation;

public sealed class DigitalTwinTests
{
    [Fact]
    public void SimulateHazardPropagation_Fire_ProducesFireSmokeEvacuation()
    {
        IReadOnlyList<PropagationStage> chain = DigitalTwinSimulationEngine.SimulateHazardPropagation(PropagationStage.Fire);

        chain.Should().Equal(PropagationStage.Fire, PropagationStage.Smoke, PropagationStage.Evacuation);
    }

    [Fact]
    public void SimulateSeverityProgression_MultipleInjuries_EscalatesToCatastrophic()
    {
        SeverityLevel severity = DigitalTwinSimulationEngine.SimulateSeverityProgression(HazardType.Other, employeeInjuryCount: 3, machineFailureCount: 0, eventChainLength: 1);

        severity.Should().Be(SeverityLevel.Catastrophic);
    }

    [Fact]
    public void SimulateWorkflow_SevereContext_TriggersEvacuation()
    {
        var context = new WorkflowContext(HazardType.Fire, SeverityLevel.Severe, EvacuationScope.Building, EmployeeExposureCount: 0, MachineFailureCount: 0, EventChainLength: 1);

        WorkflowDecision decision = DigitalTwinSimulationEngine.SimulateWorkflow(context);

        decision.EvacuationTriggers.Should().ContainSingle(t => t.Scope == EvacuationScope.Building);
    }

    [Fact]
    public void SimulateMultiHazardChain_PreservesOrderAndIncrementsTimestamps()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var stages = new (PropagationStage, SeverityLevel)[]
        {
            (PropagationStage.Spill, SeverityLevel.Moderate),
            (PropagationStage.Fire, SeverityLevel.Severe),
            (PropagationStage.Injury, SeverityLevel.Catastrophic),
        };

        var chain = DigitalTwinSimulationEngine.SimulateMultiHazardChain(stages, start);

        chain.Nodes.Select(n => n.Stage).Should().Equal(PropagationStage.Spill, PropagationStage.Fire, PropagationStage.Injury);
        chain.Nodes[1].OccurredAtUtc.Should().Be(start.AddMinutes(1));
        chain.OverallSeverity.Should().Be(SeverityLevel.Catastrophic);
    }

    [Fact]
    public void SimulateFacilityLifecycle_ActivateFromPending_ReturnsActive()
    {
        FacilityLifecycleStatus result = DigitalTwinSimulationEngine.SimulateFacilityLifecycle(FacilityLifecycleStatus.PendingActivation, FacilityLifecycleAction.Activate);

        result.Should().Be(FacilityLifecycleStatus.Active);
    }

    [Fact]
    public void SimulateFacilityLifecycle_ReopenFromShutDown_ReturnsActive()
    {
        FacilityLifecycleStatus result = DigitalTwinSimulationEngine.SimulateFacilityLifecycle(FacilityLifecycleStatus.ShutDown, FacilityLifecycleAction.Reopen);

        result.Should().Be(FacilityLifecycleStatus.Active);
    }

    [Fact]
    public void SimulateFacilityLifecycle_InvalidTransition_Throws()
    {
        Action act = () => DigitalTwinSimulationEngine.SimulateFacilityLifecycle(FacilityLifecycleStatus.Decommissioned, FacilityLifecycleAction.Activate);

        act.Should().Throw<InvalidOperationException>();
    }
}
