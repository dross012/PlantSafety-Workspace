// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Common
//  Layer:          Domain
//  File:           LookupEntry.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical base record for lookup / reference-data entities.
//      Module-specific lookups (e.g. AlertTypeLookup) extend this type.
//      Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Common.Types.Lookups;

/// <summary>Base record for lookup/reference-data entries used across modules.</summary>
public abstract record LookupEntry
{
    /// <summary>Unique machine-readable code (e.g. "FIRE", "CHEMICAL_SPILL").</summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>Human-readable display label.</summary>
    public string Label { get; init; } = string.Empty;

    /// <summary>Optional description for UI tooltips and documentation.</summary>
    public string? Description { get; init; }

    /// <summary>Soft-delete flag. Inactive entries are hidden from UI pick-lists.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Display ordering within a category.</summary>
    public int SortOrder { get; init; }
}
