// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport
//  Layer:          Domain
//  File:           DischargeMonitoringReportInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a DischargeMonitoringReport lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Exceptions;

/// <summary>Thrown when a <see cref="DischargeMonitoringReport"/> lifecycle transition is not permitted.</summary>
public sealed class DischargeMonitoringReportInvalidStatusTransitionException : DischargeMonitoringReportDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public DischargeMonitoringReportInvalidStatusTransitionException(
        DischargeMonitoringReportId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'DischargeMonitoringReport' {id} from '{from}' to '{to}'.",
            "DischargeMonitoringReport.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}