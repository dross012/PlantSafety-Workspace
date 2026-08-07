// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Classification
//  Layer:          Application
//  File:           HazmatMaterialDto.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single HazmatMaterial record. Backs both
//                  GetHazmatMaterialQuery and ListClassifiedMaterialsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.PHMSA.Classification.Models;

/// <summary>A single HazmatMaterial read-model record.</summary>
public sealed record HazmatMaterialDto
{
    /// <summary>Record identifier.</summary>
    public required Guid HazmatMaterialId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Draft/Classified/Superseded/Retired).</summary>
    public required string Status { get; init; }

    /// <summary>DOT/UN identification number, e.g. "UN1090".</summary>
    public required string UNNumber { get; init; }

    /// <summary>DOT proper shipping name, e.g. "Acetone".</summary>
    public required string ProperShippingName { get; init; }

    /// <summary>DOT hazard class/division, e.g. "3" or "6.1".</summary>
    public required string HazardClass { get; init; }

    /// <summary>DOT packing group (I/II/III/NotApplicable).</summary>
    public required string PackingGroup { get; init; }

    /// <summary>Free-text DOT special provision codes.</summary>
    public IReadOnlyList<string> SpecialProvisions { get; init; } = [];

    /// <summary>Emergency Response Guidebook 3-digit guide number, if assigned.</summary>
    public string? ERGGuideNumber { get; init; }

    /// <summary>Whether the material is classified as a marine pollutant.</summary>
    public bool IsMarinePollutant { get; init; }

    /// <summary>Whether the material is a Toxic Inhalation Hazard (TIH/PIH).</summary>
    public bool IsTIH { get; init; }

    /// <summary>TIH zone (A-D), if applicable.</summary>
    public string? TIHZone { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
