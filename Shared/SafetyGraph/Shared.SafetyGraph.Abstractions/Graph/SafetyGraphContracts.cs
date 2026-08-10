// ============================================================================
//  Project:        Shared.SafetyGraph.Abstractions
//  Module:         Graph
//  File:           SafetyGraphContracts.cs
//  Created:        2026-08-10
//  Description:    Phase 16 deliverable. Unified schema request/result
//                  contracts for graph construction and full graph
//                  evaluation. The construction request carries the real,
//                  upstream cross-module outputs -- EventRoutingResult
//                  (Phase 13), SchedulingRuleDecision (Phase 14), and
//                  PredictiveSafetyEvaluationResultContract (Phase 15) --
//                  unchanged, rather than re-deriving them; Facility/
//                  Employee/Machine/Workflow are represented by their real
//                  stable identifiers only (no invented business fields),
//                  since Shared.SafetyGraph.Abstractions does not take a
//                  dependency on Shared.EmployeeDomain/Shared.MachineDomain/
//                  Shared.WorkflowEngine for this phase -- only the
//                  identifiers those domains already expose are needed to
//                  place a deterministic node in the graph.
// ============================================================================
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.SafetyData.Abstractions.Models.Industries.Eventing;
using Shared.SafetyGraph.Abstractions.Inference;
using Shared.Scheduling.Abstractions.Rules;

namespace Shared.SafetyGraph.Abstractions.Graph;

/// <summary>The deterministic, raw cross-module input contract to a graph construction.</summary>
public sealed record SafetyGraphConstructionRequestContract(
    SafetyGraphMetadata Metadata,
    Guid FacilityId,
    string FacilityName,
    Guid? EmployeeId,
    Guid MachineId,
    string? WorkflowId,
    EventRoutingResult? RoutingResult,
    SchedulingRuleDecision? SchedulingDecision,
    PredictiveSafetyEvaluationResultContract? PredictiveResult);

/// <summary>The deterministic output contract of a graph construction: the assembled SafetyGraph.</summary>
public sealed record SafetyGraphConstructionResultContract(
    SafetyGraphMetadata Metadata,
    SafetyGraphModel Graph);

/// <summary>The deterministic input contract to a full graph evaluation (construction + traversal + inference).</summary>
public sealed record SafetyGraphEvaluationRequestContract(
    SafetyGraphMetadata Metadata,
    SafetyGraphConstructionRequestContract ConstructionRequest);

/// <summary>The deterministic output contract of a full graph evaluation: the assembled graph plus its inference result.</summary>
public sealed record SafetyGraphEvaluationResultContract(
    SafetyGraphMetadata Metadata,
    SafetyGraphModel Graph,
    SafetyGraphInferenceResult Inference);
