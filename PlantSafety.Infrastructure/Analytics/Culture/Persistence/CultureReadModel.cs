// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Culture
//  Layer:          Infrastructure
//  File:           CultureReadModel.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Dapper read model DTO for Culture queries.
//                  Additive-only evolution.
//  UPDATED:        2026-07-27 -- added SiteId/SurveyPeriod/CultureIndexScore/EngagementScore/
//                  ParticipationRate/TopRiskTheme/ETag. usp_Culture_GetById/GetAll always selected
//                  these columns (SurveyPeriod etc. since the procedures' creation; ETag added
//                  alongside the new RowVersion column) but this record didn't declare them, so
//                  Dapper silently dropped them during materialization rather than erroring --
//                  the read model didn't actually reflect the row shape it was reading.
// ============================================================================
namespace PlantSafety.Infrastructure.Analytics.Culture.Persistence;

/// <summary>Dapper read model returned by Culture query stored procedures.</summary>
public sealed record CultureReadModel
{
    /// <summary>Aggregate identifier.</summary>
    public Guid CultureId { get; init; }

    /// <summary>Current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Tenant scoping identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Optional site scoping identifier.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Survey period this Culture snapshot covers.</summary>
    public string SurveyPeriod { get; init; } = string.Empty;

    /// <summary>Composite culture index score for the period.</summary>
    public decimal CultureIndexScore { get; init; }

    /// <summary>Engagement score for the period.</summary>
    public decimal EngagementScore { get; init; }

    /// <summary>Survey participation rate for the period.</summary>
    public decimal ParticipationRate { get; init; }

    /// <summary>Top identified risk theme for the period.</summary>
    public string TopRiskTheme { get; init; } = string.Empty;

    /// <summary>Identity of the user who reviewed this survey period, if reviewed.</summary>
    public Guid? ReviewedByUserId { get; init; }

    /// <summary>UTC instant the survey period was reviewed.</summary>
    public DateTime? ReviewedAtUtc { get; init; }

    /// <summary>Identity of the user who published this survey period, if published.</summary>
    public Guid? PublishedByUserId { get; init; }

    /// <summary>UTC instant the survey period was published.</summary>
    public DateTime? PublishedAtUtc { get; init; }

    /// <summary>Identity of the user who closed this survey period, if closed.</summary>
    public Guid? ClosedByUserId { get; init; }

    /// <summary>UTC instant the survey period was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the survey period was archived.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

    /// <summary>UTC creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the creating user.</summary>
    public Guid CreatedBy { get; init; }

    /// <summary>UTC timestamp when the record was last updated, if ever.</summary>
    public DateTime? UpdatedAtUtc { get; init; }

    /// <summary>Identity of the user who last updated the record, if ever.</summary>
    public Guid? UpdatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token, projected from the row's native ROWVERSION column
    /// (<c>CONVERT(NVARCHAR(34), CAST(RowVersion AS BINARY(8)), 1)</c>). Not yet consumed by any
    /// write path -- <c>ICultureRepository</c> (Application-layer) is Create/Read-only today --
    /// but present so a future <c>UpdateAsync</c> has it available without another read-model
    /// change.
    /// </summary>
    public string ETag { get; init; } = string.Empty;
}
