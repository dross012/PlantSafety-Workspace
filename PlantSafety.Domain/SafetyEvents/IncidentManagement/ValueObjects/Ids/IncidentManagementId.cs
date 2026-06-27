// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.IncidentManagement
//  Layer:          Domain
//  File:           IncidentManagementId.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyEvents.IncidentManagement.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the IncidentManagement aggregate root.</summary>
public readonly record struct IncidentManagementId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a IncidentManagementId wrapping a non-empty GUID.</summary>
    public IncidentManagementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("IncidentManagementId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new IncidentManagementId using Uuid7.</summary>
    public static IncidentManagementId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a IncidentManagementId.</summary>
    public static IncidentManagementId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a IncidentManagementId.</summary>
    public static IncidentManagementId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}
