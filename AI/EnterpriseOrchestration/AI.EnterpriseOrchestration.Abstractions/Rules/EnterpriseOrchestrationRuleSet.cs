// ============================================================================
//  Project:        AI.EnterpriseOrchestration
//  Module:         Abstractions.Rules
//  File:           EnterpriseOrchestrationRuleSet.cs
//  Created:        2026-08-09
//  Description:    The standard, deterministic rule set for
//                  EnterpriseOrchestrationEngine.Orchestrate.
// ============================================================================
using Shared.FacilityDomain.Abstractions.Severity;

using Enterprise.WorkflowOrchestration.Abstractions.Steps;

namespace AI.EnterpriseOrchestration.Abstractions.Rules;

/// <summary>The standard rule set applied to every enterprise orchestration session.</summary>
public static class EnterpriseOrchestrationRuleSet
{
    /// <summary>The standard, deterministic rules.</summary>
    public static IReadOnlyList<EnterpriseOrchestrationRule> Standard { get; } =
    [
        new EnterpriseOrchestrationRule(
            "ORC-001",
            "Multiple facilities catastrophic",
            facilities => facilities.Count(f => f.DigitalTwinSimulatedSeverity == SeverityLevel.Catastrophic) >= 2,
            "Two or more facilities have catastrophic digital-twin projections; escalate to enterprise crisis command."),

        new EnterpriseOrchestrationRule(
            "ORC-002",
            "Concurrent shutdowns",
            facilities => facilities.Count(f => f.Workflow.ShutdownRestartSequence.Any(step => step.StepType == EnterpriseWorkflowStepType.ShutdownTrigger)) >= 2,
            "Two or more facilities are shutting down concurrently; verify no single point of failure (shared supplier, shared utility) is driving all of them."),

        new EnterpriseOrchestrationRule(
            "ORC-003",
            "Enterprise-wide regulatory exposure",
            facilities => facilities.SelectMany(f => f.Regulatory.Interpretations).Select(i => i.Jurisdiction).Distinct().Count() >= 3,
            "Three or more distinct regulatory jurisdictions apply across the enterprise; centralize regulatory filing coordination."),

        new EnterpriseOrchestrationRule(
            "ORC-004",
            "Widespread same-target optimization need",
            facilities => facilities.Count > 1 && facilities.Select(f => f.Predictive.RecommendedFocusTarget).Distinct().Count() == 1,
            "Every participating facility shares the same recommended optimization focus; treat it as an enterprise-wide initiative, not a per-facility fix."),

        new EnterpriseOrchestrationRule(
            "ORC-005",
            "Majority severe or worse",
            facilities => facilities.Count > 0 && facilities.Count(f => f.DigitalTwinSimulatedSeverity is SeverityLevel.Severe or SeverityLevel.Catastrophic) > facilities.Count / 2,
            "More than half of participating facilities are at severe or catastrophic projected severity; this is an enterprise-wide event, not isolated incidents."),
    ];
}
