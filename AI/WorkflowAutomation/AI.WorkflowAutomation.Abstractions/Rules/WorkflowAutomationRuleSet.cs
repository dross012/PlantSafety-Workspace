// ============================================================================
//  Project:        AI.WorkflowAutomation
//  Module:         Abstractions.Rules
//  File:           WorkflowAutomationRuleSet.cs
//  Created:        2026-08-09
//  Description:    The standard, deterministic shutdown/restart rule set
//                  for WorkflowAutomationEngine.Automate (Phase E.3).
// ============================================================================
using Shared.FacilityDomain.Abstractions.Severity;

using Enterprise.WorkflowOrchestration.Abstractions.Steps;

using AI.WorkflowAutomation.Abstractions.Automation;

namespace AI.WorkflowAutomation.Abstractions.Rules;

/// <summary>The standard rule set applied to every workflow automation.</summary>
public static class WorkflowAutomationRuleSet
{
    /// <summary>The standard, deterministic rules.</summary>
    public static IReadOnlyList<WorkflowAutomationRule> Standard { get; } =
    [
        new WorkflowAutomationRule(
            "WFA-001",
            "Requested shutdown",
            input => input.RequiresShutdown,
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.ShutdownTrigger, "SHUTDOWN-REQUESTED", "Shutdown requested by the incident context.")),

        new WorkflowAutomationRule(
            "WFA-002",
            "Requested restart",
            input => input.RequiresRestart,
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.RestartTrigger, "RESTART-REQUESTED", "Restart requested by the incident context.")),

        new WorkflowAutomationRule(
            "WFA-003",
            "Catastrophic severity forces shutdown",
            input => !input.RequiresShutdown && input.Context.SeverityLevel == SeverityLevel.Catastrophic,
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.ShutdownTrigger, "SHUTDOWN-CATASTROPHIC", "Catastrophic severity forces a shutdown even though none was explicitly requested.")),

        new WorkflowAutomationRule(
            "WFA-004",
            "Multiple machine failures force shutdown",
            input => !input.RequiresShutdown && input.Context.MachineFailureCount >= 2,
            new EnterpriseWorkflowStep(EnterpriseWorkflowStepType.ShutdownTrigger, "SHUTDOWN-MACHINE-FAILURES", "Two or more machine failures force a shutdown pending inspection.")),
    ];
}
