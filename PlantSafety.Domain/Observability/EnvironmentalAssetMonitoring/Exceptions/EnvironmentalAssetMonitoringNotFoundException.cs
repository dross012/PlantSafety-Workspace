// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Observability.EnvironmentalAssetMonitoring
//  Layer:          Domain
//  File:           EnvironmentalAssetMonitoringNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a EnvironmentalAssetMonitoring aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Observability.EnvironmentalAssetMonitoring.Exceptions;

/// <summary>Thrown when a <see cref="EnvironmentalAssetMonitoring"/> aggregate lookup finds no match.</summary>
public sealed class EnvironmentalAssetMonitoringNotFoundException : EnvironmentalAssetMonitoringDomainException
{
    /// <summary>Creates a not-found exception for the given EnvironmentalAssetMonitoring identity.</summary>
    public EnvironmentalAssetMonitoringNotFoundException(EnvironmentalAssetMonitoringId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"EnvironmentalAssetMonitoring with id '{id}' was not found.",
            "EnvironmentalAssetMonitoring.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}