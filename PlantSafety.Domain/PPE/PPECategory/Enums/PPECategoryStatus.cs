// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPECategory
//  Layer:          Domain
//  File:           PPECategoryStatus.cs
//  Created:        2026-07-20
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the PPECategory aggregate root.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPECategory.Enums;

/// <summary>Lifecycle states for the <see cref="PPECategory"/> aggregate root.</summary>
public enum PPECategoryStatus
{
    /// <summary>Record has been created and awaits processing.</summary>
    Created = 0,

    /// <summary>Record is active and under management.</summary>
    Active = 1,

    /// <summary>Record has been completed or resolved.</summary>
    Completed = 2,

    /// <summary>Record has been closed with no further action required.</summary>
    Closed = 3,

    /// <summary>Record has been archived for long-term retention.</summary>
    Archived = 4,
}