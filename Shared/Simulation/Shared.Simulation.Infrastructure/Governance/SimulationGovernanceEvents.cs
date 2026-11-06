// ============================================================================
//  Project:        Shared.Simulation.Infrastructure
//  Module:         Governance
//  File:           SimulationGovernanceEvents.cs
//  Created:        2026-08-10
//  Description:    Phase 17 deliverable. The fixed set of EventId literals
//                  Simulation's telemetry and governance layers write,
//                  mirroring SafetyGraphGovernanceEvents (Phase 16) exactly.
// ============================================================================
namespace Shared.Simulation.Infrastructure.Governance;

/// <summary>The fixed set of EventId literals Simulation's telemetry and governance layers write.</summary>
public static class SimulationGovernanceEvents
{
    /// <summary>Emitted by SimulationTelemetry after a scenario replay completes.</summary>
    public const string ReplayPerformed = "SIMULATION_REPLAY_PERFORMED";

    /// <summary>Emitted by SimulationTelemetry after a scenario evaluation completes.</summary>
    public const string ScenarioEvaluated = "SIMULATION_SCENARIO_EVALUATED";

    /// <summary>Emitted by SimulationEventEmitter after a simulation evaluation result event is published.</summary>
    public const string EvaluationEventPublished = "SIMULATION_EVALUATION_EVENT_PUBLISHED";

    /// <summary>Emitted by SimulationGovernanceEvaluator when a governance rule violation is found.</summary>
    public const string GovernanceViolation = "SIMULATION_GOVERNANCE_VIOLATION";

    /// <summary>Emitted by SimulationOrchestrator once a full evaluation completes.</summary>
    public const string OrchestrationCompleted = "SIMULATION_ORCHESTRATION_COMPLETED";

    /// <summary>Emitted by SimulationTracing for each recorded orchestration span.</summary>
    public const string TraceSpan = "SIMULATION_TRACE_SPAN";
}
