// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.PpePolicies
//  Layer:          Domain
//  File:           PpePoliciesInvalidStatusTransitionException.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Module Batch)
//  SchemaVersion:  2026.1
//  Description:    Thrown when a PpePolicies lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.PpePolicies.Exceptions;

/// <summary>Thrown when a <see cref="PpePolicies"/> lifecycle transition is not permitted.</summary>
public sealed class PpePoliciesInvalidStatusTransitionException : PpePoliciesDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public PpePoliciesInvalidStatusTransitionException(
        PpePoliciesId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            "Cannot transition 'PpePolicies' {id} from '{from}' to '{to}'.",
            "PpePolicies.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}