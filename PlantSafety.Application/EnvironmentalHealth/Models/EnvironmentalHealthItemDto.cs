// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Application
//  File:           EnvironmentalHealthItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single EnvironmentalHealth record. Backs
//                  GetEnvironmentalHealthByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Models.AuditingItemDto.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.EnvironmentalHealth.Models;

/// <summary>A single EnvironmentalHealth read-model record.</summary>
public sealed record EnvironmentalHealthItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid EnvironmentalHealthId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Environmental monitoring classification (e.g. AirQuality/WaterQuality/NoiseLevel/SoilContamination).</summary>
    public required string MonitoringType { get; init; }

    /// <summary>Measured value for the monitored metric, when captured.</summary>
    public decimal? MeasuredValue { get; init; }

    /// <summary>Unit of measure for <see cref="MeasuredValue"/>.</summary>
    public required string Unit { get; init; }

    /// <summary>Regulatory/internal threshold this measurement is evaluated against.</summary>
    public decimal Threshold { get; init; }

    /// <summary>Whether the measured value exceeded <see cref="Threshold"/>.</summary>
    public bool ThresholdExceeded { get; init; }

    /// <summary>Evaluated business classification: Normal, Warning, Critical.</summary>
    public required string EvaluatedStatus { get; init; }

    /// <summary>UTC timestamp the reading was recorded.</summary>
    public DateTime? RecordedAtUtc { get; init; }

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
