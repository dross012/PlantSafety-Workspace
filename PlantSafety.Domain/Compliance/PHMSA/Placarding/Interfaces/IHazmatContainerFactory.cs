// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Placarding
//  Layer:          Domain
//  File:           IHazmatContainerFactory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing HazmatContainer aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Placarding.Enums;

namespace PlantSafety.Domain.Compliance.PHMSA.Placarding.Interfaces;

/// <summary>Factory interface for constructing <see cref="HazmatContainer"/> aggregates.</summary>
public interface IHazmatContainerFactory
{
    /// <summary>Creates a new HazmatContainer aggregate with the supplied parameters.</summary>
    HazmatContainer Create(
        Guid tenantId,
        Guid? siteId,
        ContainerType containerType,
        HazmatMaterialId materialId,
        PackagingSpecificationId packagingSpecId,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
