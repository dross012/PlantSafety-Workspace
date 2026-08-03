// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Domain
//  File:           WorkflowEventId.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for a raised Workflow domain event.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Identifiers;

namespace PlantSafety.Domain.Workflow.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for a raised Workflow domain event.</summary>
public readonly record struct WorkflowEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WorkflowEventId wrapping a non-empty GUID.</summary>
    public WorkflowEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkflowEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WorkflowEventId using Uuid7.</summary>
    public static WorkflowEventId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WorkflowEventId.</summary>
    public static WorkflowEventId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a WorkflowEventId.</summary>
    public static WorkflowEventId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
