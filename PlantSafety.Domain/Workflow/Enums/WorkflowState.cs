// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowState.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum representing the explicit, finite set of overall lifecycle states for
//                  a WorkflowInstance (Phase 5.2 workflow runtime: deterministic state machine,
//                  explicit states rather than implicit strings).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Workflow.Enums;

/// <summary>Smart Enum representing the explicit overall lifecycle state of a WorkflowInstance.</summary>
public sealed class WorkflowState : IEquatable<WorkflowState>
{
    /// <summary>Instance has been created but its first step has not yet been activated.</summary>
    public static readonly WorkflowState NotStarted = new("NotStarted", "Not Started", isTerminal: false);

    /// <summary>Instance has an actively executing step.</summary>
    public static readonly WorkflowState InProgress = new("InProgress", "In Progress", isTerminal: false);

    /// <summary>Instance is blocked awaiting a human approval decision.</summary>
    public static readonly WorkflowState WaitingForApproval = new("WaitingForApproval", "Waiting For Approval", isTerminal: false);

    /// <summary>Instance's current step has been escalated to a designated escalation actor.</summary>
    public static readonly WorkflowState Escalated = new("Escalated", "Escalated", isTerminal: false);

    /// <summary>Instance's current step exceeded its timeout window and is pending escalation/retry resolution.</summary>
    public static readonly WorkflowState TimedOut = new("TimedOut", "Timed Out", isTerminal: false);

    /// <summary>Instance's current step failed and a bounded retry has been scheduled.</summary>
    public static readonly WorkflowState Retrying = new("Retrying", "Retrying", isTerminal: false);

    /// <summary>Instance completed all steps successfully. Terminal state.</summary>
    public static readonly WorkflowState Completed = new("Completed", "Completed", isTerminal: true);

    /// <summary>Instance was rejected at a human-approval step. Terminal state.</summary>
    public static readonly WorkflowState Rejected = new("Rejected", "Rejected", isTerminal: true);

    /// <summary>Instance was cancelled before completion. Terminal state.</summary>
    public static readonly WorkflowState Cancelled = new("Cancelled", "Cancelled", isTerminal: true);

    /// <summary>Instance exhausted its bounded retry budget and could not complete. Terminal state.</summary>
    public static readonly WorkflowState Failed = new("Failed", "Failed", isTerminal: true);

    /// <summary>All defined WorkflowState instances.</summary>
    public static readonly IReadOnlyList<WorkflowState> All =
        [NotStarted, InProgress, WaitingForApproval, Escalated, TimedOut, Retrying, Completed, Rejected, Cancelled, Failed];

    /// <summary>The default WorkflowState instance, used when no explicit state is supplied.</summary>
    public static WorkflowState Default => NotStarted;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    /// <summary>Whether this state is terminal (no further transitions are permitted).</summary>
    public bool IsTerminal { get; }

    private WorkflowState(string code, string displayName, bool isTerminal)
    {
        Code = code;
        DisplayName = displayName;
        IsTerminal = isTerminal;
    }

    /// <summary>Resolves a WorkflowState instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static WorkflowState From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown WorkflowState code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a WorkflowState instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out WorkflowState value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized WorkflowState code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(WorkflowState? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as WorkflowState);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
