// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowType.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Smart Enum for the category of a WorkflowDefinition: Approvals, Escalations,
//                  Notifications, and Multi-step processes (Phase 5.1 workflow definitions).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Workflow.Enums;

/// <summary>Smart Enum representing the governing category taxonomy for a WorkflowDefinition.</summary>
public sealed class WorkflowType : IEquatable<WorkflowType>
{
    /// <summary>A single- or multi-step approval workflow (e.g. permit sign-off).</summary>
    public static readonly WorkflowType Approval = new("Approval", "Approval Workflow");

    /// <summary>A workflow whose purpose is to escalate an unresolved condition to a higher authority.</summary>
    public static readonly WorkflowType Escalation = new("Escalation", "Escalation Workflow");

    /// <summary>A workflow whose purpose is to notify one or more actors of a condition.</summary>
    public static readonly WorkflowType Notification = new("Notification", "Notification Workflow");

    /// <summary>A general-purpose multi-step process combining approvals, automation, and notifications.</summary>
    public static readonly WorkflowType MultiStepProcess = new("MultiStepProcess", "Multi-Step Process");

    /// <summary>All defined WorkflowType instances.</summary>
    public static readonly IReadOnlyList<WorkflowType> All = [Approval, Escalation, Notification, MultiStepProcess];

    /// <summary>The default WorkflowType instance, used when no explicit type is supplied.</summary>
    public static WorkflowType Default => MultiStepProcess;

    /// <summary>Stable machine-readable code.</summary>
    public string Code { get; }

    /// <summary>Human-readable display name.</summary>
    public string DisplayName { get; }

    private WorkflowType(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }

    /// <summary>Resolves a WorkflowType instance from its <see cref="Code"/>. Throws if unknown.</summary>
    public static WorkflowType From(string code)
        => TryParse(code, out var value)
            ? value
            : throw new ArgumentException($"Unknown WorkflowType code '{code}'.", nameof(code));

    /// <summary>Attempts to resolve a WorkflowType instance from its <see cref="Code"/>.</summary>
    public static bool TryParse(string? code, out WorkflowType value)
    {
        value = All.FirstOrDefault(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase))!;
        return value is not null;
    }

    /// <summary>Returns <see langword="true"/> when <paramref name="code"/> is a recognized WorkflowType code.</summary>
    public static bool IsValid(string? code) => TryParse(code, out _);

    /// <inheritdoc/>
    public bool Equals(WorkflowType? other) => other is not null && Code == other.Code;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as WorkflowType);

    /// <inheritdoc/>
    public override int GetHashCode() => Code.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Code;
}
