// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a DischargeMonitoringReport aggregate lookup finds no match. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Exceptions;

/// <summary>Thrown when a <see cref="DischargeMonitoringReport"/> aggregate lookup finds no match.</summary>
public sealed class DischargeMonitoringReportNotFoundException : DischargeMonitoringReportDomainException
{
    /// <summary>Creates a not-found exception for the given DischargeMonitoringReport identity.</summary>
    public DischargeMonitoringReportNotFoundException(DischargeMonitoringReportId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"DischargeMonitoringReport with id '{id}' was not found.",
            "DischargeMonitoringReport.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}