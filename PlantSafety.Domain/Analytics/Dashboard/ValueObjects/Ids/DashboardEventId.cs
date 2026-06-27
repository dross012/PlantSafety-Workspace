// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Domain
//  File:           DashboardEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Analytics.Dashboard.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for Dashboard domain events.</summary>
public readonly record struct DashboardEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DashboardEventId wrapping a non-empty GUID.</summary>
    public DashboardEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DashboardEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DashboardEventId using Uuid7.</summary>
    public static DashboardEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
