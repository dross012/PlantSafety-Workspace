// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicyNotFoundException.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Thrown when a RestrictedZonePolicy aggregate lookup finds no match.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Exceptions;

/// <summary>Thrown when a <see cref="RestrictedZonePolicy"/> aggregate lookup finds no match.</summary>
public sealed class RestrictedZonePolicyNotFoundException : RestrictedZonePolicyDomainException
{
    /// <summary>Creates a not-found exception for the given RestrictedZonePolicy identity.</summary>
    public RestrictedZonePolicyNotFoundException(RestrictedZonePolicyId id, CorrelationId correlationId, CausationId causationId)
        : base(
            $"RestrictedZonePolicy with id '{id}' was not found.",
            "RestrictedZonePolicy.NotFound",
            "NotFound",
            "Medium",
            correlationId,
            causationId)
    {
    }
}
