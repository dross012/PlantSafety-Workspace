// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the WorkOrderPriority aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the WorkOrderPriority aggregate root.</summary>
public readonly record struct WorkOrderPriorityId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WorkOrderPriorityId wrapping a non-empty GUID.</summary>
    public WorkOrderPriorityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkOrderPriorityId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WorkOrderPriorityId using Uuid7.</summary>
    public static WorkOrderPriorityId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WorkOrderPriorityId.</summary>
    public static WorkOrderPriorityId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a WorkOrderPriorityId.</summary>
    public static WorkOrderPriorityId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}