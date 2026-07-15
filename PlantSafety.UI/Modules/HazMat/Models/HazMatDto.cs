// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a single HazMat (Hazardous Materials)
//   read-model record. Mirrors PlantSafety.Application.Operational.
//   HazardousMaterials.Models.HazardousMaterialsItemDto field-for-field so
//   System.Text.Json can deserialize the API response without a manual
//   mapping step. No domain logic present — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.HazMat.Models;

/// <summary>A single HazMat read-model record returned by the API.</summary>
public sealed class HazMatDto
{
    /// <summary>Gets the HazMat record identifier.</summary>
    public Guid HazardousMaterialId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the name of the hazardous material.</summary>
    public string MaterialName { get; init; } = string.Empty;

    /// <summary>Gets the hazard classification (e.g., GHS class).</summary>
    public string HazardClass { get; init; } = string.Empty;

    /// <summary>Gets the current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this record was last actioned.</summary>
    public DateTime RegisteredAtUtc { get; init; }

    /// <summary>Gets the identity of the user who last actioned this record.</summary>
    public Guid RegisteredBy { get; init; }
}
