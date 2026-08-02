// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           ScoreCardsItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single ScoreCards record. Backs both
//                  GetScoreCardsByIdQuery and GetScoreCardsQuery. Distinct from the pre-existing,
//                  simpler category-level DTOs under Analytics.ScoreCards.{Compliance,Department,
//                  Supervisor,Training}.Models. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.ScoreCards.Models;

/// <summary>A single ScoreCards read-model record.</summary>
public sealed record ScoreCardsItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid ScoreCardsId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable scorecard name.</summary>
    public required string ScoreCardName { get; init; }

    /// <summary>Reporting period this scorecard covers (e.g., "2026-Q3").</summary>
    public required string ReportingPeriod { get; init; }

    /// <summary>KPI category this scorecard reports on (e.g., Safety, Compliance, Operations).</summary>
    public required string KpiCategory { get; init; }

    /// <summary>Computed performance index (0-100).</summary>
    public required decimal PerformanceIndex { get; init; }

    /// <summary>Site this scorecard was generated for, if site-scoped. Null means enterprise-wide.</summary>
    public Guid? GeneratedForSiteId { get; init; }

    /// <summary>UTC timestamp this scorecard was generated.</summary>
    public required DateTime GeneratedAtUtc { get; init; }

    /// <summary>Identity of the user who published this scorecard, if published.</summary>
    public Guid? PublishedByUserId { get; init; }

    /// <summary>UTC instant the scorecard was published.</summary>
    public DateTime? PublishedAtUtc { get; init; }

    /// <summary>Identity of the user who finalized this scorecard, if finalized.</summary>
    public Guid? FinalizedByUserId { get; init; }

    /// <summary>UTC instant the scorecard was finalized.</summary>
    public DateTime? FinalizedAtUtc { get; init; }

    /// <summary>Identity of the user who closed this scorecard, if closed.</summary>
    public Guid? ClosedByUserId { get; init; }

    /// <summary>UTC instant the scorecard was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the scorecard was archived for long-term retention.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

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
