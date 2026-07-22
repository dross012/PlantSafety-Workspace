// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoringEventId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for EnvironmentalAssetMonitoring domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for EnvironmentalAssetMonitoring domain events.</summary>
public readonly record struct EnvironmentalAssetMonitoringEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EnvironmentalAssetMonitoringEventId wrapping a non-empty GUID.</summary>
    public EnvironmentalAssetMonitoringEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EnvironmentalAssetMonitoringEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EnvironmentalAssetMonitoringEventId using Uuid7.</summary>
    public static EnvironmentalAssetMonitoringEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}