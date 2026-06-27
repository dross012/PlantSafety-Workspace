// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagementEventId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for EmergencyManagement domain events.</summary>
public readonly record struct EmergencyManagementEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EmergencyManagementEventId wrapping a non-empty GUID.</summary>
    public EmergencyManagementEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmergencyManagementEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EmergencyManagementEventId using Uuid7.</summary>
    public static EmergencyManagementEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
