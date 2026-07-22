// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PPETrainingFit record. PPE training and fit testing: donning/doffing, respirator fit testing, care & maintenance, competency records.
// SchemaVersion: 1

namespace PlantSafety.Application.PPE.PPETrainingFit.Models;

/// <summary>A single PPETrainingFit read-model record.</summary>
public sealed record PPETrainingFitItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PPETrainingFitId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>WorkerId.</summary>
    public required  Guid WorkerId { get; init; }

    /// <summary>TrainingType.</summary>
    public required  string TrainingType { get; init; }

    /// <summary>FitTestExpiresAtUtc.</summary>
    public DateTime? FitTestExpiresAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }
}