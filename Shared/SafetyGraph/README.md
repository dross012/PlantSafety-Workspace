# Shared.SafetyGraph — Phase 16 (Safety Graph Model)

A deterministic, immutable, unified cross-module graph representation of PlantSafety's domain:
Facility, Hazard, Employee, Machine, Workflow, RoutingDecision, SchedulingDecision,
DiagnosticsAnomaly, and PredictiveInference nodes, connected by Causal, Dependency, Temporal,
Spatial, Organizational, and Predictive edges — built, traversed, and reasoned about with the same
deterministic-rules, decorator-chain, authorization/tenancy/telemetry/governance conventions every
phase since Phase 8 has used.

## A naming note before anything else

The graph aggregate type is named `SafetyGraphModel`, not `SafetyGraph`, even though the file is
still `SafetyGraph.cs` (matching this phase's own required output structure). This project's root
namespace is `Shared.SafetyGraph`; C#'s simple-name lookup resolves an unqualified `SafetyGraph`
to that enclosing namespace before ever considering a type of the same name (`CS0118: 'SafetyGraph'
is a namespace but is used like a type`) — confirmed by an actual failed build while writing this
module, not assumed. Every other type in this module keeps its expected name.

## Layout

```
Shared/SafetyGraph/
  Shared.SafetyGraph.Abstractions/
    Graph/SafetyGraph.cs            -- SafetyGraphModel (the aggregate), query helpers
    Graph/SafetyGraphNode.cs        -- 9-value NodeType enum + SafetyGraphNode
    Graph/SafetyGraphEdge.cs        -- 6-value EdgeType enum + SafetyGraphEdge
    Graph/SafetyGraphMetadata.cs    -- unified schema metadata envelope
    Graph/SafetyGraphContracts.cs   -- construction/evaluation request+result contracts
    Construction/SafetyGraphBuilder.cs             -- deterministic per-node/per-edge factories
    Construction/SafetyGraphAssembler.cs           -- composes a full graph, deterministic ordering
    Construction/SafetyGraphMetadataExtractor.cs   -- pure metadata extraction
    Traversal/SafetyGraphTraversalEngine.cs        -- DFS, BFS, paths, cycles, reachability, dependencies
    Traversal/SafetyGraphTraversalRules.cs         -- deterministic sorted-adjacency construction
    Traversal/SafetyGraphTraversalResult.cs
    Inference/SafetyGraphInferenceEngine.cs        -- hazard/maintenance/workflow propagation + escalation
    Inference/SafetyGraphInferenceRules.cs
    Inference/SafetyGraphInferenceResult.cs
    Contracts/SafetyGraphDto.cs, SafetyGraphErrors.cs
    Governance/SafetyGraphGovernanceRules.cs       -- pure governance validation, incl. structural integrity
  Shared.SafetyGraph.Infrastructure/
    Orchestration/SafetyGraphOrchestrator.cs           -- the one public entry point
    Orchestration/SafetyGraphEvaluationPipeline.cs     -- fixed Construct -> Infer composition
    Orchestration/SafetyGraphEventEmitter.cs           -- unified event schema publication
    Authorization/SafetyGraphAuthorization.cs          -- Phase 9 integration
    Authentication/SafetyGraphIdentity.cs              -- Phase 10 integration
    Tenant/SafetyGraphTenant.cs                         -- Phase 11 + Phase 12 integration
    Telemetry/{SafetyGraphTelemetry,SafetyGraphMetrics,SafetyGraphTracing}.cs -- Phase 8 integration
    Governance/{SafetyGraphGovernanceEvaluator,SafetyGraphGovernanceEvents}.cs
    DependencyInjection/SafetyGraphServiceCollectionExtensions.cs
  Shared.SafetyGraph.Tests/
    ... one test class per file above, mirroring Shared.PredictiveSafety.Tests' structure
```

## The graph itself

`SafetyGraphModel` is an immutable record: a `SafetyGraphMetadata` envelope plus a
deterministically-sorted `Nodes` list (by `NodeId`, ordinal) and `Edges` list (by `SourceNodeId`,
then `TargetNodeId`, then `EdgeType`, then `EdgeId`, ordinal). Every `NodeId`/`EdgeId` is a
deterministic composite key (`SafetyGraphBuilder`'s exact format, e.g. `"Facility:{facilityId}"`,
`"RoutingDecision:{eventCode}:{facilityId}"`, `"{SourceNodeId}->{TargetNodeId}:{EdgeType}"`) — never
a random `Guid` — so the same underlying entity always produces the same node/edge identity across
separate graph constructions.

`SafetyGraphAssembler.Assemble` builds the graph from a `SafetyGraphConstructionRequestContract`
carrying the **real, unchanged** outputs of earlier phases directly: `EventRoutingResult` (Phase 13),
`SchedulingRuleDecision` (Phase 14), and `PredictiveSafetyEvaluationResultContract` (Phase 15).
Facility/Employee/Machine/Workflow are represented by their real stable identifiers only — no
invented business fields, and no new `ProjectReference` on `Shared.EmployeeDomain`/
`Shared.MachineDomain`/`Shared.WorkflowEngine` was needed to place a deterministic node for them.

## Traversal

`SafetyGraphTraversalEngine` is pure, deterministic graph algorithms only — no heuristics, no
probabilistic search, no ML:

- `TraverseDepthFirst` / `TraverseBreadthFirst` — classic DFS/BFS over adjacency lists that
  `SafetyGraphTraversalRules.BuildAdjacency` sorts by target `NodeId` first, so visitation order is
  100% reproducible for the same graph and start node.
- `EnumeratePaths` — DFS-based enumeration of every simple (non-repeating) path between two nodes.
- `DetectCycle` — a classic white/gray/black DFS over every node in the graph, in `NodeId` order,
  returning the first cycle found (deterministically, since node visitation order is fixed).
- `ComputeReachableNodeIds` — the DFS visited set from a start node.
- `ResolveDependencies` — reachability restricted to `Dependency`-typed edges only.

## Inference

`SafetyGraphInferenceEngine.Infer` does **not** re-implement hazard/maintenance/routing/scheduling/
audit rule evaluation — that would duplicate Phase 15's already-real, already-deterministic logic.
Instead it evaluates what the graph's **topology** adds on top: hazard propagation (DFS from every
Hazard node via `Causal` edges), maintenance propagation (from every Machine node via `Dependency`
edges), and workflow propagation (from every Workflow node via `Dependency` edges), each optionally
flagged by whether a graph-wide cycle intersects it. `SafetyGraphInferenceRules.Evaluate` combines
these with the real, upstream `PredictiveSafetyEvaluationResultContract.OverallPriority` (Phase 15,
unchanged) into a `PredictiveEscalationOutcome` — reusing Phase 15's own
`Shared.PredictiveSafety.Abstractions.Common.PredictiveEscalationOutcome`/`PredictivePriority`/
`PredictiveEscalationClassifier` types directly, rather than inventing a parallel priority
vocabulary.

## The decorator chain

Identical in shape to `SchedulingOrchestrator`/`PredictiveSafetyOrchestrator` (Phases 14/15):

```
SafetyGraphOrchestrator.EvaluateAsync
  -> SafetyGraphTenant.EvaluateWithTenantEnforcementAsync      (Phase 11 tenant isolation + Phase 12 contract governance)
    -> SafetyGraphAuthorization.EvaluateWithAuthorizationAsync (Phase 9 authorization check, "SafetyGraph.Read")
      -> SafetyGraphEvaluationPipeline.EvaluateAsync           (fixed Construct -> Infer composition)
        -> SafetyGraphTelemetry.ConstructWithTelemetryAsync / InferWithTelemetryAsync (Phase 8, one per stage)
          -> SafetyGraphAssembler.Assemble / SafetyGraphInferenceEngine.Infer         (pure domain logic, this phase)
```

`SafetyGraphOrchestrator.EvaluateAsync` then runs governance validation
(`SafetyGraphGovernanceEvaluator` — contract metadata **and** graph structural integrity: every
edge must reference a real node, and both lists must be sorted), publishes the unified event
(`SafetyGraphEventEmitter.Emit`), and records a deterministic trace span
(`SafetyGraphTracing.RecordSpan`) — all in the same fixed order for every call.

## Metrics and log categories added this phase

- `LogCategory.SafetyGraph` (`Shared.Telemetry.Abstractions.Events.LogCategory`)
- `LogCategoryClassifier.Classify("graph")` -> `LogCategory.SafetyGraph`
- `PlantSafetyMetrics.SafetyGraphsConstructed`
- `PlantSafetyMetrics.SafetyGraphTraversalsPerformed`
- `PlantSafetyMetrics.SafetyGraphInferencesComputed`
- `PlantSafetyMetrics.SafetyGraphGovernanceViolationsDetected`

No new Meter instance was created — every Safety Graph counter is published through the same
`PlantSafetyMetrics.Meter` every other module's telemetry already uses.
