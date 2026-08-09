// ============================================================================
//  Project:        AI.WorkflowAutomation
//  Module:         Abstractions.Rules
//  File:           WorkflowAutomationRule.cs
//  Created:        2026-08-08
//  Description:    Phase E.3: a single deterministic rule generating a
//                  shutdown/restart EnterpriseWorkflowStep when its typed
//                  Condition is satisfied. Required actions/SOP/permit/
//                  evacuation steps are generated directly by Shared.
//                  WorkflowEngine's WorkflowEngine.Evaluate (Phase C.7,
//                  already deterministic) -- this rule type exists only
//                  for the two enterprise-level triggers (Shutdown/Restart,
//                  Phase D.6) that engine doesn't produce.
// ============================================================================
using Enterprise.WorkflowOrchestration.Abstractions.Steps;

using AI.WorkflowAutomation.Abstractions.Automation;

namespace AI.WorkflowAutomation.Abstractions.Rules;

/// <summary>A single deterministic rule generating a shutdown/restart step when its condition is satisfied.</summary>
public sealed record WorkflowAutomationRule(
    string RuleId,
    string Name,
    Func<WorkflowAutomationInput, bool> Condition,
    EnterpriseWorkflowStep GeneratedStep);
