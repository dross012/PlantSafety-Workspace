// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Domain
//  File:           OperationalSafetyInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when an OperationalSafety lifecycle transition is not permitted.
//                  Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Exceptions.AuditingInvalidStatusTransitionException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.OperationalSafety.Exceptions;

/// <summary>Thrown when a <see cref="OperationalSafety"/> lifecycle transition is not permitted.</summary>
public sealed class OperationalSafetyInvalidStatusTransitionException : OperationalSafetyDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public OperationalSafetyInvalidStatusTransitionException(
        OperationalSafetyId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'OperationalSafety' {id} from '{from}' to '{to}'.",
            "OperationalSafety.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
