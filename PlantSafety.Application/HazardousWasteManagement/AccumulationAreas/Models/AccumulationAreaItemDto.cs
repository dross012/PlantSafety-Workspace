// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-model item DTO for a single AccumulationArea record.
// SchemaVersion: 1

namespace PlantSafety.Application.HazardousWasteManagement.AccumulationAreas.Models;

/// <summary>A single hazardous waste accumulation area record.</summary>
public sealed record AccumulationAreaItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid AccumulationAreaId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable name of the accumulation area.</summary>
    public required string AreaName { get; init; }

    /// <summary>Physical location description within the site.</summary>
    public required string LocationDescription { get; init; }

    /// <summary>Default RCRA-style accumulation time limit in days.</summary>
    public required int TimeLimitDays { get; init; }

    /// <summary>Overall status classification.</summary>
    public required string Status { get; init; }

    /// <summary>Earliest regulatory deadline among the area's open containers, if any.</summary>
    public DateTime? EarliestContainerDeadlineUtc { get; init; }
}
