// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           EngagementItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single Engagement record. Backs both
//                  GetEngagementByIdQuery and GetEngagementRecordsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Analytics.Engagement.Models;

/// <summary>A single Engagement read-model record.</summary>
public sealed record EngagementItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EngagementId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Worker this engagement measurement relates to.</summary>
    public required Guid WorkerId { get; init; }

    /// <summary>Computed engagement score (0-100).</summary>
    public required decimal EngagementScore { get; init; }

    /// <summary>Survey period this measurement covers (e.g., "2026-Q3").</summary>
    public required string SurveyPeriod { get; init; }

    /// <summary>Channel through which engagement was captured (e.g., MobileApp, Kiosk, Paper, Interview).</summary>
    public required string ParticipationChannel { get; init; }

    /// <summary>Number of responses/interactions captured for this measurement.</summary>
    public required int ResponseCount { get; init; }

    /// <summary>UTC timestamp this worker last engaged with a safety touchpoint.</summary>
    public DateTime? LastEngagedAtUtc { get; init; }

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
