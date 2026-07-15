// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Domain
//  File:           ProcessDefinitionId.cs
//  Created:        2026-07-15
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the ProcessDefinition aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ProcessManagement.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the ProcessDefinition aggregate root.</summary>
public readonly record struct ProcessDefinitionId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a ProcessDefinitionId wrapping a non-empty GUID.</summary>
    public ProcessDefinitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProcessDefinitionId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new ProcessDefinitionId using Uuid7.</summary>
    public static ProcessDefinitionId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a ProcessDefinitionId.</summary>
    public static ProcessDefinitionId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a ProcessDefinitionId.</summary>
    public static ProcessDefinitionId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
