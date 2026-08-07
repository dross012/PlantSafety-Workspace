// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           HazmatMaterialFactory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Default factory implementation for constructing HazmatMaterial aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Interfaces;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Factories;

/// <summary>Default factory implementation for constructing <c>HazmatMaterial</c> aggregates.</summary>
public sealed class HazmatMaterialFactory : IHazmatMaterialFactory
{
    /// <inheritdoc/>
    public HazmatMaterial Create(
        UNNumber unNumber,
        ProperShippingName shippingName,
        HazardClass hazardClass,
        PackingGroup packingGroup,
        IReadOnlyList<string> specialProvisions,
        string? ergGuideNumber,
        bool isMarinePollutant,
        bool isTIH,
        TIHZone? tihZone,
        UserId createdBy,
        UtcTimestamp createdAtUtc,
        CorrelationId correlationId,
        CausationId causationId,
        PerformedByActor actor,
        Guid tenantId,
        Guid? siteId = null)
        => HazmatMaterial.Create(
            unNumber, shippingName, hazardClass, packingGroup, specialProvisions, ergGuideNumber,
            isMarinePollutant, isTIH, tihZone, createdBy, createdAtUtc, correlationId, causationId, actor,
            tenantId, siteId);
}
