// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowStepStatus.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for a single WorkflowStepInstance child entity.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Workflow.Enums;

/// <summary>Lifecycle states for a single WorkflowStepInstance child entity.</summary>
public enum WorkflowStepStatus
{
    /// <summary>Step has been created but is not yet the active step.</summary>
    Pending = 0,

    /// <summary>Step is the currently active step, awaiting resolution.</summary>
    Active = 1,

    /// <summary>Human-approval step was approved.</summary>
    Approved = 2,

    /// <summary>Human-approval step was rejected.</summary>
    Rejected = 3,

    /// <summary>Automated-decision step resolved to a positive outcome.</summary>
    AutomatedDecisionMade = 4,

    /// <summary>Predictive-recommendation step surfaced a recommendation for downstream action.</summary>
    RecommendationSurfaced = 5,

    /// <summary>Step exceeded its configured timeout window.</summary>
    TimedOut = 6,

    /// <summary>Step was escalated to a designated escalation actor after timeout.</summary>
    Escalated = 7,

    /// <summary>Step failed and a bounded retry has been scheduled.</summary>
    Retrying = 8,

    /// <summary>Step completed successfully (notification sent, decision recorded, etc.).</summary>
    Completed = 9,

    /// <summary>Step was skipped because its entry guard condition was not satisfied.</summary>
    Skipped = 10,

    /// <summary>Step was cancelled along with its parent instance.</summary>
    Cancelled = 11,
}
