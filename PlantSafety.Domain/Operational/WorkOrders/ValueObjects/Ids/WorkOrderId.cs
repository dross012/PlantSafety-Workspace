// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Domain
//  File:           WorkOrderId.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the WorkOrder aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

/// <summary>Strong-typed identity for the WorkOrder aggregate root.</summary>
public readonly record struct WorkOrderId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a WorkOrderId wrapping a non-empty GUID.</summary>
    public WorkOrderId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("WorkOrderId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new WorkOrderId using Uuid7.</summary>
    public static WorkOrderId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a WorkOrderId.</summary>
    public static WorkOrderId FromGuid(Guid value) => new(value);

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
