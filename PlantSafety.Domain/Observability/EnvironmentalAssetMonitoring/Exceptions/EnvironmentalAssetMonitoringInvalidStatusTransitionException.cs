// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoringInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EnvironmentalAssetMonitoring lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Exceptions;

/// <summary>Thrown when a <see cref="EnvironmentalAssetMonitoring"/> lifecycle transition is not permitted.</summary>
public sealed class EnvironmentalAssetMonitoringInvalidStatusTransitionException : EnvironmentalAssetMonitoringDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public EnvironmentalAssetMonitoringInvalidStatusTransitionException(
        EnvironmentalAssetMonitoringId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'EnvironmentalAssetMonitoring' {id} from '{from}' to '{to}'.",
            "EnvironmentalAssetMonitoring.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}