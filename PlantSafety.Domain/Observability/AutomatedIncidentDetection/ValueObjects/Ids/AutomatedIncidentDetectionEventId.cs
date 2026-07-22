// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.AutomatedIncidentDetection
//  Layer:          Domain
//  File:           AutomatedIncidentDetectionEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for AutomatedIncidentDetection domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.AutomatedIncidentDetection.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for AutomatedIncidentDetection domain events.</summary>
public readonly record struct AutomatedIncidentDetectionEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a AutomatedIncidentDetectionEventId wrapping a non-empty GUID.</summary>
    public AutomatedIncidentDetectionEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AutomatedIncidentDetectionEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new AutomatedIncidentDetectionEventId using Uuid7.</summary>
    public static AutomatedIncidentDetectionEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}