// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the AutomatedIncidentDetection aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the AutomatedIncidentDetection aggregate root.</summary>
public readonly record struct AutomatedIncidentDetectionId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AutomatedIncidentDetectionId wrapping a non-empty GUID.</summary>
    public AutomatedIncidentDetectionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AutomatedIncidentDetectionId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AutomatedIncidentDetectionId using Uuid7.</summary>
    public static AutomatedIncidentDetectionId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a AutomatedIncidentDetectionId.</summary>
    public static AutomatedIncidentDetectionId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a AutomatedIncidentDetectionId.</summary>
    public static AutomatedIncidentDetectionId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}