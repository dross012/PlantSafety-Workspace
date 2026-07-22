// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a AuditEngagement lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Exceptions;

/// <summary>Thrown when a <see cref="AuditEngagement"/> lifecycle transition is not permitted.</summary>
public sealed class AuditEngagementInvalidStatusTransitionException : AuditEngagementDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AuditEngagementInvalidStatusTransitionException(
        AuditEngagementId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'AuditEngagement' " + id + " from '" + from + "' to '" + to + "'.",
            "AuditEngagement.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}