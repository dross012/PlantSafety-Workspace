// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Application
//  File:           SustainabilityItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single Sustainability record. Backs
//                  GetSustainabilityByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Models.AuditingItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Sustainability.Models;

/// <summary>A single Sustainability read-model record.</summary>
public sealed record SustainabilityItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid SustainabilityId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable sustainability/ESG initiative name.</summary>
    public required string InitiativeName { get; init; }

    /// <summary>Initiative category (e.g., Emissions, WasteReduction, EnergyEfficiency, WaterConservation).</summary>
    public required string Category { get; init; }

    /// <summary>Optional target value for the initiative's tracked metric.</summary>
    public decimal? TargetValue { get; init; }

    /// <summary>Identity of the accountable initiative owner.</summary>
    public Guid? OwnerUserId { get; init; }

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
