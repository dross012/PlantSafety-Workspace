// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoringId.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Strong-typed identity value object for the EnvironmentalAssetMonitoring aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the EnvironmentalAssetMonitoring aggregate root.</summary>
public readonly record struct EnvironmentalAssetMonitoringId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a EnvironmentalAssetMonitoringId wrapping a non-empty GUID.</summary>
    public EnvironmentalAssetMonitoringId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EnvironmentalAssetMonitoringId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new EnvironmentalAssetMonitoringId using Uuid7.</summary>
    public static EnvironmentalAssetMonitoringId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a EnvironmentalAssetMonitoringId.</summary>
    public static EnvironmentalAssetMonitoringId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a EnvironmentalAssetMonitoringId.</summary>
    public static EnvironmentalAssetMonitoringId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}