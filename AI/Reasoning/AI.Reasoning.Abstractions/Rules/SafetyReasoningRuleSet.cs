// ============================================================================
//  Project:        AI.Reasoning
//  Module:         Abstractions.Rules
//  File:           SafetyReasoningRuleSet.cs
//  Created:        2026-08-09
//  Description:    The standard, deterministic rule set for
//                  SafetyReasoningEngine.Reason.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Lifecycle;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;

using Enterprise.WorkflowOrchestration.Abstractions.States;

namespace AI.Reasoning.Abstractions.Rules;

/// <summary>The standard rule set applied to every safety reasoning session.</summary>
public static class SafetyReasoningRuleSet
{
    /// <summary>The standard, deterministic rules.</summary>
    public static IReadOnlyList<SafetyReasoningRule> Standard { get; } =
    [
        new SafetyReasoningRule(
            "RSN-001",
            "Active facility with injuries",
            ctx => ctx.Input.FacilityStatus == FacilityLifecycleStatus.Active && ctx.Input.EmployeeInjuryCount > 0,
            "Facility remains active despite reported employee injuries; verify the affected area has been isolated."),

        new SafetyReasoningRule(
            "RSN-002",
            "Multi-jurisdiction exposure",
            ctx => ctx.Input.ApplicableJurisdictions.Count >= 2,
            "Multiple regulatory jurisdictions apply; coordinate reporting across all of them from a single incident record."),

        new SafetyReasoningRule(
            "RSN-003",
            "Workflow not yet escalated despite severe severity",
            ctx => ctx.Input.SeverityLevel is SeverityLevel.Severe or SeverityLevel.Catastrophic && ctx.Input.WorkflowState == EnterpriseWorkflowState.Pending,
            "Severity is severe or catastrophic but the enterprise workflow is still only Pending; escalation is overdue."),

        new SafetyReasoningRule(
            "RSN-004",
            "Digital twin projects worse outcome",
            ctx => ctx.Input.DigitalTwinSimulatedSeverity > ctx.Input.SeverityLevel,
            "The digital twin simulates a worse outcome than the currently observed severity; treat the twin's projection as the planning basis."),

        new SafetyReasoningRule(
            "RSN-005",
            "Connected-facility exposure",
            ctx => ctx.Input.EnterpriseGraphConnectedFacilityCount > 0 && ctx.Input.SeverityLevel is SeverityLevel.Severe or SeverityLevel.Catastrophic,
            "This facility has enterprise-graph connections to other facilities; a severe/catastrophic event here may propagate to them."),

        new SafetyReasoningRule(
            "RSN-006",
            "Compounding failures",
            ctx => ctx.Input.EmployeeInjuryCount > 0 && ctx.Input.MachineFailureCount > 0,
            "Both employee injury and machine failure are present; treat this as a compound incident, not two independent ones."),
    ];
}
