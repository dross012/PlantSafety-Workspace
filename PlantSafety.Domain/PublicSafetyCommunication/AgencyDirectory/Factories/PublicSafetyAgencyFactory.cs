// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PublicSafetyCommunication.AgencyDirectory
//  Layer:          Domain
//  File:           PublicSafetyAgencyFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing PublicSafetyAgency aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Factories;

/// <summary>Default factory implementation for constructing <see cref="PublicSafetyAgency"/> aggregates.</summary>
public sealed class PublicSafetyAgencyFactory : IPublicSafetyAgencyFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency Create(
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.Aggregates.PublicSafetyAgency.Create(createdBy, createdAtUtc, correlationId, causationId, actor);
}
