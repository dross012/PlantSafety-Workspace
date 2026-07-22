// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyProcedures.MaterialHandlingStorage
//  Layer:          Domain
//  File:           MaterialHandlingCategory.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Classification of material handling / storage procedure categories per the
//                  PlantSafety Program Structure Material Handling & Storage component.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.SafetyProcedures.MaterialHandlingStorage.Enums;

/// <summary>Classification of material handling / storage procedure categories.</summary>
public enum MaterialHandlingCategory
{
    /// <summary>Safe manual lifting techniques.</summary>
    SafeLifting = 0,

    /// <summary>Forklift operation rules.</summary>
    ForkliftOperation = 1,

    /// <summary>Pallet jack operation rules.</summary>
    PalletJackOperation = 2,

    /// <summary>Crane operation rules.</summary>
    CraneOperation = 3,

    /// <summary>Material stacking/storage rules.</summary>
    MaterialStorage = 4,
}
