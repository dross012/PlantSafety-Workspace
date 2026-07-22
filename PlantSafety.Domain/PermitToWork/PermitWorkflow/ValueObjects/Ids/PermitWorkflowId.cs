// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PermitToWork.PermitWorkflow
//  Layer:          Domain
//  File:           PermitWorkflowId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the PermitWorkflow aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PermitToWork.PermitWorkflow.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the PermitWorkflow aggregate root.</summary>
public readonly record struct PermitWorkflowId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a PermitWorkflowId wrapping a non-empty GUID.</summary>
    public PermitWorkflowId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PermitWorkflowId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new PermitWorkflowId using Uuid7.</summary>
    public static PermitWorkflowId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a PermitWorkflowId.</summary>
    public static PermitWorkflowId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a PermitWorkflowId.</summary>
    public static PermitWorkflowId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}