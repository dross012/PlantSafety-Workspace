// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Domain
//  File:           GovernanceInvalidStatusTransitionException.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Thrown when a Governance lifecycle transition is not permitted. Mirrors
//                  PlantSafety.Domain.Compliance.Auditing.Exceptions.AuditingInvalidStatusTransitionException.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Governance.Exceptions;

/// <summary>Thrown when a <see cref="Governance"/> lifecycle transition is not permitted.</summary>
public sealed class GovernanceInvalidStatusTransitionException : GovernanceDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public GovernanceInvalidStatusTransitionException(
        GovernanceId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'Governance' {id} from '{from}' to '{to}'.",
            "Governance.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
