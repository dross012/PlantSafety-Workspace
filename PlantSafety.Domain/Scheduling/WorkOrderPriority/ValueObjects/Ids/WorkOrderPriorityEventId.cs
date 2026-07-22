// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scheduling.WorkOrderPriority
//  Layer:          Domain
//  File:           WorkOrderPriorityEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for WorkOrderPriority domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Scheduling.WorkOrderPriority.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for WorkOrderPriority domain events.</summary>
public readonly record struct WorkOrderPriorityEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WorkOrderPriorityEventId wrapping a non-empty GUID.</summary>
    public WorkOrderPriorityEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkOrderPriorityEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WorkOrderPriorityEventId using Uuid7.</summary>
    public static WorkOrderPriorityEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}