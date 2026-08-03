// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           AssignedActor.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Value object representing the actor (role or specific user) assigned to
//                  resolve a workflow step (Phase 5.1: "assigned roles/actors").
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Workflow.ValueObjects;

/// <summary>Identifies the role or specific user assigned to resolve a workflow step.</summary>
public readonly record struct AssignedActor
{
    /// <summary>Role code the step is assigned to (e.g. "SafetyManager"), or <see langword="null"/> when assigned to a specific user.</summary>
    public string? RoleCode { get; }

    /// <summary>Specific user the step is assigned to, or <see langword="null"/> when assigned by role.</summary>
    public UserId? UserId { get; }

    private AssignedActor(string? roleCode, UserId? userId)
    {
        RoleCode = roleCode;
        UserId = userId;
    }

    /// <summary>Creates an actor assignment scoped to any user holding the given role.</summary>
    public static AssignedActor ForRole(string roleCode)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            throw new ArgumentException("Role code must not be empty.", nameof(roleCode));
        return new AssignedActor(roleCode.Trim(), null);
    }

    /// <summary>Creates an actor assignment scoped to a specific user.</summary>
    public static AssignedActor ForUser(UserId userId) => new(null, userId);

    /// <summary>Unassigned placeholder (matches no one; used for steps requiring no human actor).</summary>
    public static AssignedActor Unassigned => new(null, null);

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="candidateUserId"/> (with
    /// <paramref name="candidateRoles"/>) is permitted to act on a step carrying this assignment.
    /// </summary>
    public bool Matches(UserId candidateUserId, IReadOnlySet<string> candidateRoles)
    {
        if (UserId is { } assignedUser)
            return assignedUser.Value == candidateUserId.Value;

        if (RoleCode is not null)
            return candidateRoles.Contains(RoleCode);

        return false;
    }

    /// <inheritdoc/>
    public override string ToString() => UserId is { } u ? $"User:{u.Value:D}" : RoleCode ?? "Unassigned";
}
