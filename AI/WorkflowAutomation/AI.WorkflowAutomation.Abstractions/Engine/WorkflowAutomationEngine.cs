// ============================================================================
//  Project:        AI.WorkflowAutomation
//  Module:         Abstractions.Engine
//  File:           WorkflowAutomationEngine.cs
//  Created:        2026-08-08
//  Description:    Phase E.3 (Path E: AI-Driven Safety Automation):
//                  generates every workflow artifact Path E's spec
//                  requires by composing the real, already-built engines:
//                    - required actions/SOP/permit/evacuation -> Shared.
//                      WorkflowEngine's WorkflowEngine.Evaluate (Phase C.7)
//                    - shutdown/restart sequence -> WorkflowAutomationRule
//                      evaluation (Phase E.3, new -- the two enterprise-
//                      level triggers Phase C.7 doesn't produce)
//                    - unified execution plan -> Enterprise.WorkflowOrchestration's
//                      EnterpriseWorkflowOrchestrator.BuildExecutionPlan (Phase D.6)
//                  Pure function, no side effects, no reflection.
// ============================================================================
using Shared.WorkflowEngine.Abstractions.Actions;
using Shared.WorkflowEngine.Abstractions.Triggers;

using Enterprise.WorkflowOrchestration.Abstractions.Orchestrator;
using Enterprise.WorkflowOrchestration.Abstractions.Steps;

using AI.WorkflowAutomation.Abstractions.Automation;
using AI.WorkflowAutomation.Abstractions.Rules;

namespace AI.WorkflowAutomation.Abstractions.Engine;

/// <summary>Generates every workflow automation artifact by composing Path C/D's real engines.</summary>
public static class WorkflowAutomationEngine
{
    /// <summary>Generates the full set of automated workflow artifacts for the given input.</summary>
    public static WorkflowAutomationOutput Automate(WorkflowAutomationInput input, IReadOnlyList<WorkflowAutomationRule> shutdownRestartRules, DateTime? nowUtc = null)
    {
        DateTime generatedAtUtc = nowUtc ?? DateTime.UtcNow;

        Shared.WorkflowEngine.Abstractions.Engine.WorkflowDecision decision = global::Shared.WorkflowEngine.Abstractions.Engine.WorkflowEngine.Evaluate(input.Context);

        IReadOnlyList<EnterpriseWorkflowStep> shutdownRestartSequence = shutdownRestartRules
            .Where(rule => rule.Condition(input))
            .Select(rule => rule.GeneratedStep)
            .ToArray();

        var allSteps = new List<EnterpriseWorkflowStep>();
        allSteps.AddRange(decision.RequiredActions.Select(ToStep));
        allSteps.AddRange(decision.SopTriggers.Select(ToStep));
        allSteps.AddRange(decision.PermitTriggers.Select(ToStep));
        allSteps.AddRange(decision.EvacuationTriggers.Select(ToStep));
        allSteps.AddRange(shutdownRestartSequence);

        IReadOnlyList<EnterpriseWorkflowStep> executionPlan = EnterpriseWorkflowOrchestrator.BuildExecutionPlan(allSteps);

        return new WorkflowAutomationOutput(
            decision.RequiredActions,
            decision.SopTriggers,
            decision.PermitTriggers,
            decision.EvacuationTriggers,
            shutdownRestartSequence,
            executionPlan,
            generatedAtUtc);
    }

    private static EnterpriseWorkflowStep ToStep(RequiredAction action) =>
        new(EnterpriseWorkflowStepType.RequiredAction, action.ActionCode, action.Description);

    private static EnterpriseWorkflowStep ToStep(SopTrigger trigger) =>
        new(EnterpriseWorkflowStepType.SopTrigger, trigger.SopCode, trigger.TriggerReason);

    private static EnterpriseWorkflowStep ToStep(PermitTrigger trigger) =>
        new(EnterpriseWorkflowStepType.PermitTrigger, trigger.PermitCode, trigger.TriggerReason);

    private static EnterpriseWorkflowStep ToStep(EvacuationTrigger trigger) =>
        new(EnterpriseWorkflowStepType.EvacuationTrigger, trigger.Scope.ToString(), trigger.Reason);
}
