// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditEngagement
//  Layer:          Domain
//  File:           AuditEngagementNotFoundException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a AuditEngagement aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditEngagement.Exceptions;

/// <summary>Thrown when a <see cref="AuditEngagement"/> aggregate lookup finds no match.</summary>
public sealed class AuditEngagementNotFoundException : AuditEngagementDomainException
{
    /// <summary>Creates a not-found exception for the given AuditEngagement identity.</summary>
    public AuditEngagementNotFoundException(AuditEngagementId id, CorrelationId correlationId, CausationId causationId)
        : base(
            "AuditEngagement with id '" + id + "' was not found.",
            "AuditEngagement.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}