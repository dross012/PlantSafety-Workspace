// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a single Lockout/Tagout (LOTO)
//   read-model record. Mirrors PlantSafety.Application.Operational.
//   LockoutTagout.Models.LockoutTagoutItemDto field-for-field so
//   System.Text.Json can deserialize the API response without a manual
//   mapping step. No domain logic present — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Loto.Models;

/// <summary>A single LOTO read-model record returned by the API.</summary>
public sealed class LotoDto
{
    /// <summary>Gets the LOTO record identifier.</summary>
    public Guid LockoutTagoutId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the identifier of the equipment under lockout/tagout.</summary>
    public Guid EquipmentId { get; init; }

    /// <summary>Gets the type of lock applied (e.g., Electrical/Mechanical).</summary>
    public string LockType { get; init; } = string.Empty;

    /// <summary>Gets the current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this record was last actioned.</summary>
    public DateTime LockedAtUtc { get; init; }

    /// <summary>Gets the identity of the user who last actioned this record.</summary>
    public Guid LockedBy { get; init; }
}
