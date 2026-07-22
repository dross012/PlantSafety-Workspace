// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContractorSafety.ActiveMonitoringAuditing
//  Layer:          Domain
//  File:           ActiveMonitoringAuditingInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a ActiveMonitoringAuditing lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.ContractorSafety.ActiveMonitoringAuditing.Exceptions;

/// <summary>Thrown when a <see cref="ActiveMonitoringAuditing"/> lifecycle transition is not permitted.</summary>
public sealed class ActiveMonitoringAuditingInvalidStatusTransitionException : ActiveMonitoringAuditingDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public ActiveMonitoringAuditingInvalidStatusTransitionException(
        ActiveMonitoringAuditingId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'ActiveMonitoringAuditing' {id} from '{from}' to '{to}'.",
            "ActiveMonitoringAuditing.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}