// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowStepId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a WorkflowStepDefinition child entity
//                  of the WorkflowDefinition aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Workflow.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a WorkflowStepDefinition child entity.</summary>
public readonly record struct WorkflowStepId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WorkflowStepId wrapping a non-empty GUID.</summary>
    public WorkflowStepId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkflowStepId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WorkflowStepId using Uuid7.</summary>
    public static WorkflowStepId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WorkflowStepId.</summary>
    public static WorkflowStepId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a WorkflowStepId.</summary>
    public static WorkflowStepId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
