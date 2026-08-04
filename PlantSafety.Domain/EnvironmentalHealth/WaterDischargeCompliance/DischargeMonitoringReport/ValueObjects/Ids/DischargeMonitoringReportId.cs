// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for the DischargeMonitoringReport aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for the DischargeMonitoringReport aggregate root.</summary>
public readonly record struct DischargeMonitoringReportId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DischargeMonitoringReportId wrapping a non-empty GUID.</summary>
    public DischargeMonitoringReportId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DischargeMonitoringReportId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DischargeMonitoringReportId using Uuid7.</summary>
    public static DischargeMonitoringReportId New() => new(Uuid7.New());

    /// <summary>Wraps an existing GUID as a DischargeMonitoringReportId.</summary>
    public static DischargeMonitoringReportId FromGuid(Guid value) => new(value);

    /// <summary>Parses a GUID string into a DischargeMonitoringReportId.</summary>
    public static DischargeMonitoringReportId Parse(string value) => new(Guid.Parse(value));

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}