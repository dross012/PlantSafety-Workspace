// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PermitTemplate record. Standard permit templates per category (Hot Work, Confined Space, Electrical/Isolation, Working at Height, Excavation) with required controls.
// SchemaVersion: 1

namespace PlantSafety.Application.PermitToWork.PermitTemplate.Models;

/// <summary>A single PermitTemplate read-model record.</summary>
public sealed record PermitTemplateItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PermitTemplateId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>PermitTypeCode.</summary>
    public required  string PermitTypeCode { get; init; }

    /// <summary>RequiredControls.</summary>
    public required  string RequiredControls { get; init; }

    /// <summary>StandardValidityHours.</summary>
    public required  int StandardValidityHours { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}