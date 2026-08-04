// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacilityFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Default factory implementation for constructing TsdfFacility aggregates.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Interfaces;

namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Factories;

/// <summary>Default factory implementation for constructing TsdfFacility aggregates.</summary>
public sealed class TsdfFacilityFactory : ITsdfFacilityFactory
{
    /// <inheritdoc/>
    public PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Aggregates.TsdfFacility Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, string facilityName, string epaFacilityId, UtcTimestamp? permitExpiresAtUtc)
        => PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Aggregates.TsdfFacility.Create(
            createdBy, createdAtUtc, correlationId, causationId, actor, tenantId, siteId, facilityName, epaFacilityId, permitExpiresAtUtc);
}
