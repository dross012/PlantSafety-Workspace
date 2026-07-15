// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO for a Safety Observation record. Mirrors
//   PlantSafety.Application.Features.SafetyEvents.Models.Observation.ObservationDto
//   returned by GET /v1/features/safety-events/observations/{id}.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;

namespace PlantSafety.UI.Modules.Observations.Models;

/// <summary>Read-side projection of a Safety Observation — a documented workplace safety condition.</summary>
public sealed class ObservationDto
{
    /// <summary>Gets the unique observation identifier (UUIDv7).</summary>
    public Guid ObservationId { get; init; }

    /// <summary>Gets the owning tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site where the observation was made.</summary>
    public Guid SiteId { get; init; }

    /// <summary>Gets the short descriptive title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the narrative description of the observed condition.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the observation type: Positive, AtRisk, Unsafe.</summary>
    public string ObservationType { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when the observation was made.</summary>
    public DateTimeOffset ObservedAtUtc { get; init; }

    /// <summary>Gets the worker who made the observation.</summary>
    public Guid ObservedByWorkerId { get; init; }

    /// <summary>Gets the current workflow status (e.g., Open, ActionRequired, Closed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of last modification.</summary>
    public DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Gets the row version for concurrency.</summary>
    public string ETag { get; init; } = string.Empty;
}
