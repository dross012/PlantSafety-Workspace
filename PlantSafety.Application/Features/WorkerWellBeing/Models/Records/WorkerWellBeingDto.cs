// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Read-side DTO for a Worker WellBeing record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.WorkerWellBeing.Models.Records;

/// <summary>
/// Read-side projection of a WorkerWellBeing aggregate. Represents a single worker
/// wellbeing assessment (fatigue/stress-derived risk classification) and its workflow status.
/// </summary>
public sealed record WorkerWellBeingDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid WorkerWellBeingId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Worker this record relates to.</summary>
    public required Guid WorkerId { get; init; }

    /// <summary>Wellbeing risk classification (Low/Medium/High/Critical).</summary>
    public required string WellBeingRiskLevel { get; init; }

    /// <summary>Current workflow status (Open/UnderReview/Actioned/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp this record was last actioned/recorded.</summary>
    public required DateTimeOffset RecordedAtUtc { get; init; }
}
