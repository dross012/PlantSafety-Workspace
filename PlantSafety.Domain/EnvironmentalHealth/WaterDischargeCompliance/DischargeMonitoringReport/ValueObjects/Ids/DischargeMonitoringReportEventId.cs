// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportEventId.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Strong-typed, Uuid7-backed identity for DischargeMonitoringReport domain events.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.ValueObjects.Ids;

/// <summary>Strong-typed, Uuid7-backed identity for DischargeMonitoringReport domain events.</summary>
public readonly record struct DischargeMonitoringReportEventId
{
    /// <summary>Underlying GUID value.</summary>
    public Guid Value { get; }

    /// <summary>Creates a DischargeMonitoringReportEventId wrapping a non-empty GUID.</summary>
    public DischargeMonitoringReportEventId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("DischargeMonitoringReportEventId must not be empty.", nameof(value));
        Value = value;
    }

    /// <summary>Creates a new DischargeMonitoringReportEventId using Uuid7.</summary>
    public static DischargeMonitoringReportEventId New() => new(Uuid7.New());

    /// <inheritdoc/>
    public override string ToString() => Value.ToString("D");
}