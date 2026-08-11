// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single Hazardous Materials & Chemical Safety Checklists record.
// SchemaVersion: 1

namespace PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Models;

/// <summary>A single Hazardous Materials &amp; Chemical Safety Checklists read-model row.</summary>
public sealed record HazardousMaterialsAndChemicalSafetyItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid ItemId { get; init; }

    /// <summary>Identifier of the parent ChecklistTemplate aggregate this row was projected from.</summary>
    public required Guid ChecklistTemplateId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short title/summary for this row.</summary>
    public required string Title { get; init; }

    /// <summary>Current status of the underlying record (e.g. Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp this row was recorded/observed.</summary>
    public required DateTime RecordedAtUtc { get; init; }
}