// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowStepInstanceId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a WorkflowStepInstance child entity of
//                  the WorkflowInstance aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Workflow.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a WorkflowStepInstance child entity.</summary>
public readonly record struct WorkflowStepInstanceId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WorkflowStepInstanceId wrapping a non-empty GUID.</summary>
    public WorkflowStepInstanceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkflowStepInstanceId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WorkflowStepInstanceId using Uuid7.</summary>
    public static WorkflowStepInstanceId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WorkflowStepInstanceId.</summary>
    public static WorkflowStepInstanceId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a WorkflowStepInstanceId.</summary>
    public static WorkflowStepInstanceId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
