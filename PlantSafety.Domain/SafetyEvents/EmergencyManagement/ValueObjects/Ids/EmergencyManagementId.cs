// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Domain
//  File:           EmergencyManagementId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.EmergencyManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EmergencyManagement aggregate root.</summary>
public readonly record struct EmergencyManagementId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EmergencyManagementId wrapping a non-empty GUID.</summary>
    public EmergencyManagementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmergencyManagementId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EmergencyManagementId using Uuid7.</summary>
    public static EmergencyManagementId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a EmergencyManagementId.</summary>
    public static EmergencyManagementId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a EmergencyManagementId.</summary>
    public static EmergencyManagementId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
