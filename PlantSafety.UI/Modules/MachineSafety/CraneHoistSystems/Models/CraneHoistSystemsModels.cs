// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTOs for the MachineSafety.CraneHoistSystems sub-aggregate,
//   mirroring PlantSafety.Application.MachineSafety.CraneHoistSystems.Models.
//   CraneHoistSystemsItemDto field-for-field. Display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.MachineSafety.CraneHoistSystems.Models;

/// <summary>A single crane/hoist asset record (asset registry, load-limit enforcement, inspection,
/// operator qualification, and rigging safety).</summary>
public sealed class CraneHoistSystemsItemDto
{
    /// <summary>Gets the record identifier.</summary>
    public Guid CraneHoistSystemsId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the equipment tag for this asset.</summary>
    public string AssetTag { get; init; } = string.Empty;

    /// <summary>Gets the asset title.</summary>
    public string AssetTitle { get; init; } = string.Empty;

    /// <summary>Gets the classification.</summary>
    public string AssetType { get; init; } = string.Empty;

    /// <summary>Gets the manufacturer-rated maximum load capacity, in kilograms.</summary>
    public decimal RatedLoadCapacityKg { get; init; }

    /// <summary>Gets the most recently observed lift load, in kilograms.</summary>
    public decimal? LastObservedLoadKg { get; init; }

    /// <summary>Gets the count of recorded overload events over the asset's lifetime.</summary>
    public int OverloadEventCount { get; init; }

    /// <summary>Gets the UTC due date of the next scheduled inspection, if scheduled.</summary>
    public DateTime? InspectionDueAtUtc { get; init; }

    /// <summary>Gets the UTC expiry timestamp of the current operator qualification, if any.</summary>
    public DateTime? OperatorQualificationExpiresAtUtc { get; init; }

    /// <summary>Gets the overall status classification.</summary>
    public string SystemStatus { get; init; } = string.Empty;
}

/// <summary>Response payload for the crane/hoist fleet read-model query.</summary>
public sealed class CraneHoistSystemsQueryResponseDto
{
    /// <summary>Gets the matching crane/hoist asset records. Never null; empty when no results.</summary>
    public IReadOnlyList<CraneHoistSystemsItemDto> Items { get; init; } = [];
}
