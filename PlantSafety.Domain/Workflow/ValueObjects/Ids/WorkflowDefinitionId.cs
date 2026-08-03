// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowDefinitionId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the WorkflowDefinition aggregate root.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Workflow.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the WorkflowDefinition aggregate root.</summary>
public readonly record struct WorkflowDefinitionId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WorkflowDefinitionId wrapping a non-empty GUID.</summary>
    public WorkflowDefinitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkflowDefinitionId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WorkflowDefinitionId using Uuid7.</summary>
    public static WorkflowDefinitionId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WorkflowDefinitionId.</summary>
    public static WorkflowDefinitionId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a WorkflowDefinitionId.</summary>
    public static WorkflowDefinitionId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
