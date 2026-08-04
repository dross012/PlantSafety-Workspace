// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.TsdfFacilities
//  Layer:          Domain
//  File:           TsdfFacilityLifecycleRules.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Defines permitted lifecycle transitions for the TsdfFacility aggregate.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Enums;

namespace PlantSafety.Domain.HazardousWasteManagement.TsdfFacilities.Rules;

/// <summary>Defines permitted lifecycle transitions for the TsdfFacility aggregate.</summary>
public static class TsdfFacilityLifecycleRules
{
    /// <summary>Returns <see langword="true"/> when the facility may record a receipt from <paramref name="current"/>.</summary>
    public static bool CanRecordReceipt(TsdfFacilityStatus current) => current == TsdfFacilityStatus.Active;

    /// <summary>Returns <see langword="true"/> when the facility may be suspended from <paramref name="current"/>.</summary>
    public static bool CanSuspend(TsdfFacilityStatus current) => current == TsdfFacilityStatus.Active;
}
