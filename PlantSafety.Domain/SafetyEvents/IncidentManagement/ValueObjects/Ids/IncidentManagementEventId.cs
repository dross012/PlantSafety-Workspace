// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for IncidentManagement domain events.</summary>
public readonly record struct IncidentManagementEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a IncidentManagementEventId wrapping a non-empty GUID.</summary>
    public IncidentManagementEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("IncidentManagementEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new IncidentManagementEventId using Uuid7.</summary>
    public static IncidentManagementEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
