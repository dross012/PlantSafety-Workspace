// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowInstanceId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the WorkflowInstance aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Workflow.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the WorkflowInstance aggregate root.</summary>
public readonly record struct WorkflowInstanceId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WorkflowInstanceId wrapping a non-empty GUID.</summary>
    public WorkflowInstanceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkflowInstanceId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WorkflowInstanceId using Uuid7.</summary>
    public static WorkflowInstanceId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WorkflowInstanceId.</summary>
    public static WorkflowInstanceId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a WorkflowInstanceId.</summary>
    public static WorkflowInstanceId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
