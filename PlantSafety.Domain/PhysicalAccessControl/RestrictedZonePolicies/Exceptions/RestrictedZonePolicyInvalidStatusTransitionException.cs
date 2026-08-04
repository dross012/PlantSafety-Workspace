// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicyInvalidStatusTransitionException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a RestrictedZonePolicy lifecycle transition is not permitted.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Exceptions;

/// <summary>Thrown when a <see cref="RestrictedZonePolicy"/> lifecycle transition is not permitted.</summary>
public sealed class RestrictedZonePolicyInvalidStatusTransitionException : RestrictedZonePolicyDomainException
{
    /// <summary>Creates an invalid-transition exception from <paramref name="from"/> to <paramref name="to"/>.</summary>
    public RestrictedZonePolicyInvalidStatusTransitionException(
        RestrictedZonePolicyId id, string from, string to,
        CorrelationId correlationId, CausationId causationId)
        : base(
            $"Cannot transition 'RestrictedZonePolicy' {id} from '{from}' to '{to}'.",
            "RestrictedZonePolicy.InvalidTransition",
            "Lifecycle",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
