// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Read-model item DTO for a single Engagement analytics data point.
// SchemaVersion: 1

namespace PlantSafety.Application.Analytics.Models;

/// <summary>A single data point within an Engagement analytics result set.</summary>
/// <param name="Label">Human-readable label (department, date bucket, category, etc.).</param>
/// <param name="Value">Primary numeric metric value.</param>
/// <param name="Trend">Trend direction: Improving, Stable, or Declining.</param>
public sealed record EngagementDataPointDto(
    string Label,
    decimal Value,
    string? Trend);
