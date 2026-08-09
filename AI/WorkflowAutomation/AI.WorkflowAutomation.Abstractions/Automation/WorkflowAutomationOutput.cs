// ============================================================================
//  Project:        AI.WorkflowAutomation
//  Module:         Abstractions.Automation
//  File:           WorkflowAutomationOutput.cs
//  Created:        2026-08-08
//  Description:    Phase E.3: the result of a workflow-automation session
//                  -- every generated category Path E's spec requires
//                  (required actions, SOP steps, permit requirements,
//                  evacuation steps, shutdown/restart sequence), plus a
//                  single deterministic ExecutionPlan ordering all of them
//                  together (via Enterprise.WorkflowOrchestration's Phase
//                  D.6 BuildExecutionPlan).
// ============================================================================
using Shared.WorkflowEngine.Abstractions.Actions;
using Shared.WorkflowEngine.Abstractions.Triggers;

using Enterprise.WorkflowOrchestration.Abstractions.Steps;

namespace AI.WorkflowAutomation.Abstractions.Automation;

/// <summary>The result of a workflow-automation session.</summary>
public sealed record WorkflowAutomationOutput(
    IReadOnlyList<RequiredAction> RequiredActions,
    IReadOnlyList<SopTrigger> SopSteps,
    IReadOnlyList<PermitTrigger> PermitRequirements,
    IReadOnlyList<EvacuationTrigger> EvacuationSteps,
    IReadOnlyList<EnterpriseWorkflowStep> ShutdownRestartSequence,
    IReadOnlyList<EnterpriseWorkflowStep> ExecutionPlan,
    DateTime GeneratedAtUtc);
