// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.ShippingPapers
//  Layer:          Domain
//  File:           IHazmatShipmentFactory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing HazmatShipment aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Packaging.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Enums;

namespace PlantSafety.Domain.Compliance.PHMSA.ShippingPapers.Interfaces;

/// <summary>Factory interface for constructing <see cref="HazmatShipment"/> aggregates.</summary>
public interface IHazmatShipmentFactory
{
    /// <summary>Creates a new HazmatShipment aggregate with the supplied parameters.</summary>
    HazmatShipment Create(
        Guid tenantId,
        Guid? siteId,
        IReadOnlyList<HazmatMaterialId> materialIds,
        IReadOnlyList<PackagingSpecificationId> packagingIds,
        TransportMode mode,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor);
}
