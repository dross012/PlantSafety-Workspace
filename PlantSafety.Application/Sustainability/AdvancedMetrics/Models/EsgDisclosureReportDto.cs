// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           EsgDisclosureReportDto.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Read-model DTO backing GetEsgDisclosureReportByIdQuery. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Models;

/// <summary>A single ESG disclosure report read-model record.</summary>
public sealed record EsgDisclosureReportDto
{
    /// <summary>Report identifier.</summary>
    public required Guid EsgDisclosureReportId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Source ledger identifier this report was generated from.</summary>
    public required Guid SourceLedgerId { get; init; }

    /// <summary>Disclosure framework(s) this report supports (e.g. "GhgProtocol, Csrd").</summary>
    public required string Frameworks { get; init; }

    /// <summary>Reporting period start.</summary>
    public required DateTime ReportingPeriodStartUtc { get; init; }

    /// <summary>Reporting period end.</summary>
    public required DateTime ReportingPeriodEndUtc { get; init; }

    /// <summary>Report version number.</summary>
    public required int Version { get; init; }

    /// <summary>Identifier of the report this version supersedes, if any.</summary>
    public Guid? SupersedesReportId { get; init; }

    /// <summary>Calculation methodology version in force when this report was generated.</summary>
    public required string MethodologyVersion { get; init; }

    /// <summary>Current lifecycle status (Draft/Published/Superseded).</summary>
    public required string Status { get; init; }

    /// <summary>UTC instant the report was generated.</summary>
    public required DateTime GeneratedAtUtc { get; init; }

    /// <summary>Identity of the user who generated this report.</summary>
    public required Guid GeneratedBy { get; init; }

    /// <summary>UTC instant the report was published, if it has been.</summary>
    public DateTime? PublishedAtUtc { get; init; }
}
