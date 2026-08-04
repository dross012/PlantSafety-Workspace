// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTO for a single HazardousWasteManifest read-model record.
//   Mirrors PlantSafety.Application.HazardousWasteManagement.Manifests.Models.
//   HazardousWasteManifestItemDto field-for-field. Display only.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.HazardousWasteManagement.Models;

/// <summary>A single Uniform Hazardous Waste Manifest record returned by the API.</summary>
public sealed class HazardousWasteManifestDto
{
    /// <summary>Gets the manifest record identifier.</summary>
    public Guid HazardousWasteManifestId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the EPA Uniform Hazardous Waste Manifest tracking number.</summary>
    public string ManifestNumber { get; init; } = string.Empty;

    /// <summary>Gets the current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the manifest was generated.</summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp the transporter signed for custody, if recorded.</summary>
    public DateTime? TransporterSignedAtUtc { get; init; }

    /// <summary>Gets a value indicating whether this record is immutable (transporter-signed).</summary>
    public bool IsImmutable { get; init; }

    /// <summary>Gets the most recent discrepancy reason, if any.</summary>
    public string? DiscrepancyReason { get; init; }
}
