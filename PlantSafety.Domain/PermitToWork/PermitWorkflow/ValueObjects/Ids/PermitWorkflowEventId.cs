// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for PermitWorkflow domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for PermitWorkflow domain events.</summary>
public readonly record struct PermitWorkflowEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PermitWorkflowEventId wrapping a non-empty GUID.</summary>
    public PermitWorkflowEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermitWorkflowEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PermitWorkflowEventId using Uuid7.</summary>
    public static PermitWorkflowEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}