// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditFinding
//  Layer:          Domain
//  File:           AuditFindingInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a AuditFinding lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Auditing.AuditFinding.Exceptions;

/// <summary>Thrown when a <see cref="AuditFinding"/> lifecycle transition is not permitted.</summary>
public sealed class AuditFindingInvalidStatusTransitionException : AuditFindingDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public AuditFindingInvalidStatusTransitionException(
        AuditFindingId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'AuditFinding' " + id + " from '" + from + "' to '" + to + "'.",
            "AuditFinding.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}