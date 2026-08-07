// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Domain
//  File:           IHazmatMaterialFactory.cs
//  Created:        2026-08-07
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Factory interface for constructing HazmatMaterial aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Compliance.PHMSA.Classification.Aggregates;
using PlantSafety.Domain.Compliance.PHMSA.Classification.Enums;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;

namespace PlantSafety.Domain.Compliance.PHMSA.Classification.Interfaces;

/// <summary>Factory interface for constructing <c>HazmatMaterial</c> aggregates.</summary>
public interface IHazmatMaterialFactory
{
    /// <summary>Classifies a new HazmatMaterial with the supplied parameters.</summary>
    HazmatMaterial Create(
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
        Guid? siteId = null);
}
