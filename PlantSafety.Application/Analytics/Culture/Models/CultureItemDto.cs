// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Application
//  File:           CultureItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single Culture record. Backs
//                  GetCultureByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.Culture.Models;

/// <summary>A single Culture read-model record.</summary>
public sealed record CultureItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid CultureId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Survey period this measurement covers (e.g., "2026-Q3").</summary>
    public required string SurveyPeriod { get; init; }

    /// <summary>Computed safety culture index score (0-100).</summary>
    public decimal CultureIndexScore { get; init; }

    /// <summary>Computed engagement score (0-100).</summary>
    public decimal EngagementScore { get; init; }

    /// <summary>Survey participation rate (0-100).</summary>
    public decimal ParticipationRate { get; init; }

    /// <summary>Top risk theme identified from this survey period.</summary>
    public required string TopRiskTheme { get; init; }

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
