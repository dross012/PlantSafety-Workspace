// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Domain
//  File:           WorkerWellBeingId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Strong-typed Uuid7-backed identity for the WorkerWellBeing aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.WorkerWellBeing.ValueObjects;

/// <summary>Strong-typed, Uuid7-backed identity for the WorkerWellBeing aggregate root.</summary>
public readonly record struct WorkerWellBeingId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WorkerWellBeingId wrapping a non-empty GUID.</summary>
    public WorkerWellBeingId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkerWellBeingId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WorkerWellBeingId using Uuid7.</summary>
    public static WorkerWellBeingId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WorkerWellBeingId.</summary>
    public static WorkerWellBeingId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a WorkerWellBeingId.</summary>
    public static WorkerWellBeingId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
