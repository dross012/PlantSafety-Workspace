// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PhysicalAccessControl.RestrictedZonePolicies
//  Layer:          Domain
//  File:           RestrictedZonePolicyFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing RestrictedZonePolicy
//                  aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Factories;

/// <summary>Default factory implementation for constructing <see cref="RestrictedZonePolicy"/> aggregates.</summary>
public sealed class RestrictedZonePolicyFactory : IRestrictedZonePolicyFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates.RestrictedZonePolicy.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
