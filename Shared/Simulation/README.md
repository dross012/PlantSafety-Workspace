# Shared.Simulation — Phase 17 (Simulation Engine)

A deterministic, ML-free replay and scenario-evaluation subsystem: composes cross-module scenarios
out of Hazard/Maintenance/Routing/Scheduling/Workflow/PredictiveInference steps, replays them in
validated deterministic order, and evaluates them against the real, unchanged outputs of the
Predictive Safety Engine (Phase 15) and the Safety Graph Model (Phase 16) — using the same
decorator-chain/authorization/tenancy/telemetry/governance conventions every phase since Phase 8
has used.

## Two engines, two distinct jobs

`SimulationReplayEngine` and `SimulationScenarioEngine` are deliberately separate, non-overlapping
pure engines:

- **Replay** (`SimulationReplayEngine.Replay`) is about *ordering*, not outcomes. It validates that
  a scenario's `SimulationStep.StepIndex` values form a zero-based, contiguous, duplicate-free
  sequence (`SimulationReplayRules.ValidateStepOrdering`), then replays the steps in ascending
  order. This is "deterministic event ordering" and "deterministic state transitions" taken
  literally: state here is simply "which step comes next" — Simulation invents no additional domain
  state beyond what the scenario itself already carries.
- **Scenario evaluation** (`SimulationScenarioEngine.Evaluate`) is about *cross-module escalation*.
  It walks the scenario's steps in the same ascending order and, per step, asks whether the real
  upstream data says this step should escalate — **never re-deriving that answer itself**.

## What each step type actually consults

`SimulationScenarioRules.EvaluateStep` does not re-implement Phase 15's hazard/maintenance/routing/
scheduling/predictive rule evaluation — that would duplicate already-real, already-deterministic
logic. Instead:

| StepType | Escalates when |
|---|---|
| `Hazard` | `PredictiveSafetyEvaluationResultContract.Hazard.RequiresImmediateEscalation` (Phase 15, unchanged) |
| `Maintenance` | `...Maintenance.RequiresImmediateMaintenance` (Phase 15, unchanged) |
| `Routing` | `...Routing.RequiresPriorityRouting` (Phase 15, unchanged) |
| `Scheduling` | `...Scheduling.RequiresAcceleratedScheduling` (Phase 15, unchanged) |
| `Workflow` | **Actual SafetyGraph traversal** (Phase 16, unchanged): `SafetyGraphTraversalEngine.ComputeReachableNodeIds` from every `Workflow`-typed node reaches at least 2 nodes — the same `PropagationOccurredThreshold` `SafetyGraphInferenceRules` (Phase 16) already established |
| `PredictiveInference` | `...OverallPriority >= PredictivePriority.High` (Phase 15, unchanged) |

A missing `PredictiveResult`/`Graph` deterministically means "no escalation from that source" —
never an exception, never a default-to-true.

## Composition: `SimulationOutcome`

`SimulationEvaluationPipeline.EvaluateAsync` runs Replay, then Scenario evaluation, in that fixed
order, and combines them into a single `SimulationOutcome`:

```
SimulationOutcome(
    SimulationReplayResult Replay,
    SimulationScenarioResult Scenario,
    bool RequiresOverallEscalation,
    PredictivePriority OverallPriority)
```

`RequiresOverallEscalation`/`OverallPriority` are worst-case-wins across *both* engines — an
invalid replay (`IsValidReplay: false`) always forces `RequiresOverallEscalation: true` and escalates
`OverallPriority` to `Critical` if the scenario's own priority was lower, regardless of whether the
scenario itself escalated. A broken ordering is never silently ignored just because the scenario
content looked fine.

## Layout

```
Shared/Simulation/
  Shared.Simulation.Abstractions/
    Simulation/SimulationStep.cs, SimulationScenario.cs, SimulationOutcome.cs,
               SimulationMetadata.cs, SimulationContracts.cs
    Replay/SimulationReplayEngine.cs, SimulationReplayRules.cs, SimulationReplayResult.cs
    Scenario/SimulationScenarioEngine.cs, SimulationScenarioRules.cs, SimulationScenarioResult.cs
    Contracts/SimulationDto.cs, SimulationErrors.cs
    Governance/SimulationGovernanceRules.cs
  Shared.Simulation.Infrastructure/
    Orchestration/SimulationOrchestrator.cs           -- the one public entry point
    Orchestration/SimulationEvaluationPipeline.cs      -- fixed Replay -> Evaluate composition
    Orchestration/SimulationEventEmitter.cs            -- unified event schema publication
    Authorization/SimulationAuthorization.cs           -- Phase 9 integration
    Authentication/SimulationIdentity.cs                -- Phase 10 integration
    Tenant/SimulationTenant.cs                          -- Phase 11 + Phase 12 integration
    Telemetry/{SimulationTelemetry,SimulationMetrics,SimulationTracing}.cs -- Phase 8 integration
    Governance/{SimulationGovernanceEvaluator,SimulationGovernanceEvents}.cs
    DependencyInjection/SimulationServiceCollectionExtensions.cs
  Shared.Simulation.Tests/
    ... one test class per file above, mirroring Shared.SafetyGraph.Tests' structure
```

## The decorator chain

Identical in shape to `SafetyGraphOrchestrator` (Phase 16):

```
SimulationOrchestrator.EvaluateAsync
  -> SimulationTenant.EvaluateWithTenantEnforcementAsync      (Phase 11 tenant isolation + Phase 12 contract governance)
    -> SimulationAuthorization.EvaluateWithAuthorizationAsync (Phase 9 authorization check, "Simulation.Read")
      -> SimulationEvaluationPipeline.EvaluateAsync           (fixed Replay -> Evaluate composition)
        -> SimulationTelemetry.ReplayWithTelemetryAsync / EvaluateWithTelemetryAsync (Phase 8, one per stage)
          -> SimulationReplayEngine.Replay / SimulationScenarioEngine.Evaluate       (pure domain logic, this phase)
```

`SimulationOrchestrator.EvaluateAsync` then runs governance validation
(`SimulationGovernanceEvaluator` — contract metadata **and** scenario shape: non-empty ScenarioId,
at least one step), publishes the unified event (`SimulationEventEmitter.Emit`), and records a
deterministic trace span (`SimulationTracing.RecordSpan`) — all in the same fixed order for every
call.

## Cross-module dependencies

`Shared.Simulation.Abstractions` takes `ProjectReference`s on `Shared.PredictiveSafety.Abstractions`
(Phase 15's `PredictiveSafetyEvaluationResultContract`, `PredictivePriority`,
`PredictiveEscalationOutcome`/`PredictiveEscalationClassifier` — reused directly, never a parallel
vocabulary) and `Shared.SafetyGraph.Abstractions` (Phase 16's `SafetyGraphModel`,
`SafetyGraphTraversalEngine`). No new priority/escalation types were invented for this phase.

## Metrics and log categories added this phase

- `LogCategory.Simulation` (`Shared.Telemetry.Abstractions.Events.LogCategory`)
- `LogCategoryClassifier.Classify("simulation")` -> `LogCategory.Simulation`
- `PlantSafetyMetrics.SimulationReplaysPerformed`
- `PlantSafetyMetrics.SimulationScenariosEvaluated`
- `PlantSafetyMetrics.SimulationGovernanceViolationsDetected`

No new Meter instance was created — every Simulation counter is published through the same
`PlantSafetyMetrics.Meter` every other module's telemetry already uses.
