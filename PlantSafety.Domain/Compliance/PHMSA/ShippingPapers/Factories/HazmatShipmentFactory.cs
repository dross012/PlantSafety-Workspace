// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           HazmatShipmentFactory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing HazmatShipment aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Enums;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.ValueObjects.Ids;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Factories;

/// <summary>Default factory implementation for constructing <see cref="HazmatShipment"/> aggregates.</summary>
public sealed class HazmatShipmentFactory : IHazmatShipmentFactory
{
    /// <inheritdoc/>
    public HazmatShipment Create(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<HazmatMaterialId> materialIds,
        IReadOnlyList<PackagingSpecificationId> packagingIds,
        TransportMode mode,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor)
        => HazmatShipment.Create(
            HazmatShipmentId.New(),
            tenantId,
            siteId,
            materialIds,
            packagingIds,
            mode,
            createdBy,
            createdAtUtc,
            correlationId,
            causationId,
            actor);
}
