// ============================================================================
//  Project:        Enterprise.DigitalTwin
//  Module:         Abstractions.Simulation
//  File:           DigitalTwinSimulationEngine.cs
//  Created:        2026-08-08
//  Description:    Phase D.7 (Path D: Enterprise-Scale Safety Platform
//                  Modernization): answers "what would happen if" questions
//                  against digital-twin snapshots, for exactly the five
//                  simulation kinds Path D's spec requires. Deliberately
//                  composes the real, already-built deterministic rule
//                  engines from Paths B/C rather than re-implementing
//                  their logic:
//                    - Hazard propagation      -> Shared.FacilityDomain's
//                      HazardPropagationRules (Phase C.6)
//                    - Severity progression    -> Shared.FacilityDomain's
//                      SeverityInferenceRules (Phase C.3)
//                    - Workflow                -> Shared.WorkflowEngine's
//                      WorkflowEngine.Evaluate (Phase C.7)
//                    - Multi-hazard chain      -> Shared.FacilityDomain's
//                      HazardChainBuilder (Phase C.9)
//                  The one exception is facility lifecycle: FacilityAggregate's
//                  guard logic (Phase C.12) lives on a real aggregate
//                  instance and raises real domain events, which is wrong
//                  for a read-only "what if" simulation -- so
//                  SimulateFacilityLifecycle deliberately duplicates just
//                  the four guard clauses as a pure function, documented
//                  here rather than hidden, since real mutation was never
//                  appropriate for this use case.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Chains;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Lifecycle;
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Severity;

using Shared.WorkflowEngine.Abstractions.Engine;

namespace Enterprise.DigitalTwin.Abstractions.Simulation;

/// <summary>Answers "what would happen if" questions against digital-twin snapshots.</summary>
public static class DigitalTwinSimulationEngine
{
    /// <summary>Simulates the deterministic hazard propagation chain starting at <paramref name="originStage"/> (Phase C.6).</summary>
    public static IReadOnlyList<PropagationStage> SimulateHazardPropagation(PropagationStage originStage) =>
        HazardPropagationRules.BuildChain(originStage);

    /// <summary>Simulates the deterministic worst-case severity for the given hazard/injury/failure/chain context (Phase C.3).</summary>
    public static SeverityLevel SimulateSeverityProgression(HazardType hazardType, int employeeInjuryCount, int machineFailureCount, int eventChainLength) =>
        SeverityInferenceRules.Infer(hazardType, employeeInjuryCount, machineFailureCount, eventChainLength);

    /// <summary>Simulates the deterministic workflow decision for the given context (Phase C.7).</summary>
    public static WorkflowDecision SimulateWorkflow(WorkflowContext context) =>
        global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(context);

    /// <summary>Simulates a deterministic multi-hazard event chain from an ordered set of (stage, severity) pairs (Phase C.9).</summary>
    public static HazardChain SimulateMultiHazardChain(IReadOnlyList<(PropagationStage Stage, SeverityLevel Severity)> stages, DateTime startAtUtc)
    {
        var builder = new HazardChainBuilder();
        for (int i = 0; i < stages.Count; i++)
        {
            builder.AddStage(stages[i].Stage, stages[i].Severity, startAtUtc.AddMinutes(i));
        }

        return builder.Build();
    }

    /// <summary>
    /// Simulates the facility lifecycle transition that <paramref name="action"/> would produce from
    /// <paramref name="currentStatus"/>, mirroring FacilityAggregate's Phase C.12 guard clauses without
    /// mutating a real aggregate or raising real domain events.
    /// </summary>
    /// <exception cref="InvalidOperationException">The action is not valid from the current status.</exception>
    public static FacilityLifecycleStatus SimulateFacilityLifecycle(FacilityLifecycleStatus currentStatus, FacilityLifecycleAction action) => (currentStatus, action) switch
    {
        (FacilityLifecycleStatus.PendingActivation, FacilityLifecycleAction.Activate) => FacilityLifecycleStatus.Active,
        (FacilityLifecycleStatus.Active, FacilityLifecycleAction.Shutdown) => FacilityLifecycleStatus.ShutDown,
        (FacilityLifecycleStatus.ShutDown, FacilityLifecycleAction.Reopen) => FacilityLifecycleStatus.Active,
        (FacilityLifecycleStatus.Active, FacilityLifecycleAction.Decommission) => FacilityLifecycleStatus.Decommissioned,
        (FacilityLifecycleStatus.ShutDown, FacilityLifecycleAction.Decommission) => FacilityLifecycleStatus.Decommissioned,
        _ => throw new InvalidOperationException($"Cannot simulate {action} from facility status {currentStatus}."),
    };
}
