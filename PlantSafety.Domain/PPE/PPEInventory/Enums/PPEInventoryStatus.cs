// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         PPE.PPEInventory
//  Layer:          Domain
//  File:           PPEInventoryStatus.cs
//  Created:        2026-07-20
//  Updated:        2026-07-21
//  Author:         PlantSafety Platform Team (Legacy Program Structure Migration)
//  SchemaVersion:  2026.1
//  Description:    Lifecycle states for the PPEInventory aggregate root, modeling
//                  "PPE Inventory & Lifecycle Management": inventory tracking, issuance/return
//                  logs, inspection intervals, replacement thresholds, and expiration dates.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.PPE.PPEInventory.Enums;

/// <summary>Lifecycle states for the <see cref="PPEInventory"/> aggregate root.</summary>
public enum PPEInventoryStatus
{
    /// <summary>Stock levels are within normal operating range.</summary>
    InStock = 0,

    /// <summary>Available (StockQuantity - IssuedQuantity) has fallen at or below the reorder threshold.</summary>
    LowStock = 1,

    /// <summary>Item has been flagged for replacement due to inspection failure, damage, or approaching expiration.</summary>
    PendingReplacement = 2,

    /// <summary>Item has been retired from active service and may no longer be issued.</summary>
    Retired = 3,

    /// <summary>Item record has been archived for long-term retention.</summary>
    Archived = 4,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="InStock"/>.</summary>
    [Obsolete("Use InStock. Retained only for additive-only compatibility with prior scaffolding.")]
    Created = InStock,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="InStock"/>.</summary>
    [Obsolete("Use InStock. Retained only for additive-only compatibility with prior scaffolding.")]
    Active = InStock,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Retired"/>.</summary>
    [Obsolete("Use Retired. Retained only for additive-only compatibility with prior scaffolding.")]
    Completed = Retired,

    /// <summary>Legacy generic value retained for backward compatibility with earlier scaffolding. Prefer <see cref="Retired"/>.</summary>
    [Obsolete("Use Retired. Retained only for additive-only compatibility with prior scaffolding.")]
    Closed = Retired,
}
