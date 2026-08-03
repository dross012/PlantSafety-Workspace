// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowStepType.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the resolution mode of a workflow step: human approval,
//                  automated decision, predictive recommendation, or plain notification
//                  (Phase 5.3 human + machine collaboration).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Workflow.Enums;

/// <summary>Smart Enum representing how a workflow step is resolved.</summary>
public sealed class WorkflowStepType : IEquatable<WorkflowStepType>
{
    /// <summary>Step is resolved exclusively by a human actor's Approve/Reject decision.</summary>
    public static readonly WorkflowStepType HumanApproval = new("HumanApproval", "Human Approval", requiresHumanActor: true);

    /// <summary>Step is resolved automatically by a domain policy/rule without human input.</summary>
    public static readonly WorkflowStepType AutomatedDecision = new("AutomatedDecision", "Automated Decision", requiresHumanActor: false);

    /// <summary>
    /// Step surfaces a recommendation sourced from a predictive-analytics module (via SharedKernel
    /// contracts, never a direct Domain-to-Domain reference) for a subsequent human/automated step
    /// to act on.
    /// </summary>
    public static readonly WorkflowStepType PredictiveRecommendation = new("PredictiveRecommendation", "Predictive Recommendation", requiresHumanActor: false);

    /// <summary>Step performs a fire-and-forget notification with no decision to record.</summary>
    public static readonly WorkflowStepType Notification = new("Notification", "Notification", requiresHumanActor: false);

    /// <summary>Step escalates the workflow to a designated escalation actor.</summary>
    public static readonly WorkflowStepType Escalation = new("Escalation", "Escalation", requiresHumanActor: true);

    /// <summary>All defined WorkflowStepType instances.</summary>
    public static readonly IReadOnlyList<WorkflowStepType> All =
        [HumanApproval, AutomatedDecision, PredictiveRecommendation, Notification, Escalation];

    /// <summary>The default WorkflowStepType instance, used when no explicit type is supplied.</summary>
    public static WorkflowStepType Default => AutomatedDecision;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Whether this step type requires a human actor's decision to progress.</summary>
    public bool RequiresHumanActor { get; }

    private WorkflowStepType(string code, string displayName, bool requiresHumanActor)
    {
        Code = code;
        DisplayName = displayName;
        RequiresHumanActor = requiresHumanActor;
    }

    /// <summary>Resolves a WorkflowStepType instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static WorkflowStepType From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown WorkflowStepType code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a WorkflowStepType instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out WorkflowStepType value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized WorkflowStepType code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(WorkflowStepType? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as WorkflowStepType);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
