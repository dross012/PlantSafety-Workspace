// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           HazmatContainerFactory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing HazmatContainer aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Factories;

/// <summary>Default factory implementation for constructing <see cref="HazmatContainer"/> aggregates.</summary>
public sealed class HazmatContainerFactory : IHazmatContainerFactory
{
    /// <inheritdoc/>
    public HazmatContainer Create(
        Guid tenantId,
        Guid? siteId,
        ContainerType containerType,
        HazmatMaterialId materialId,
        PackagingSpecificationId packagingSpecId,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => HazmatContainer.Create(
            HazmatContainerId.New(),
            tenantId,
            siteId,
            containerType,
            materialId,
            packagingSpecId,
            createdBy,
            createdAtUtc,
            correlationId,
            causationId,
            actor);
}
