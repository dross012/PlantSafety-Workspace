// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacilityStatus.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Lifecycle states for the TsdfFacility aggregate root (a permitted hazardous-
//                  waste Treatment, Storage, and Disposal Facility).
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Enums;

/// <summary>Lifecycle states for the TsdfFacility aggregate root.</summary>
public enum TsdfFacilityStatus
{
    /// <summary>Facility active and permitted to receive waste.</summary>
    Active = 0,

    /// <summary>Facility suspended (e.g. expired RCRA permit).</summary>
    Suspended = 1,
}
