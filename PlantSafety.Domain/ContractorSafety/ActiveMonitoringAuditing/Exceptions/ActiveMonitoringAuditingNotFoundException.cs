// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ActiveMonitoringAuditing aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Exceptions;

/// <summary>Thrown when a <see cref="ActiveMonitoringAuditing"/> aggregate lookup finds no match.</summary>
public sealed class ActiveMonitoringAuditingNotFoundException : ActiveMonitoringAuditingDomainException
{
    /// <summary>Creates a not-found exception for the given ActiveMonitoringAuditing identity.</summary>
    public ActiveMonitoringAuditingNotFoundException(ActiveMonitoringAuditingId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "ActiveMonitoringAuditing with id '{id}' was not found.",
            "ActiveMonitoringAuditing.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}