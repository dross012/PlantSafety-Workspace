// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           ITsdfFacilityFactory.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Factory interface for constructing TsdfFacility aggregates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Interfaces;

/// <summary>Factory interface for constructing TsdfFacility aggregates.</summary>
public interface ITsdfFacilityFactory
{
    /// <summary>Registers a new TsdfFacility aggregate with the supplied parameters.</summary>
    PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Aggregates.TsdfFacility Create(
        UserId createdBy, UtcTimestamp createdAtUtc, CorrelationId correlationId, CausationId causationId,
        PerformedByActor actor, Guid tenantId, Guid? siteId, string facilityName, string epaFacilityId, UtcTimestamp? permitExpiresAtUtc);
}
