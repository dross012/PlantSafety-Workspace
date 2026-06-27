// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Management of Change (MOC) record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Compliance.Models;

/// <summary>
/// Read-side projection of a ManagementOfChange aggregate.
/// MOC governs changes to processes, facilities, equipment, or personnel that may affect safety.
/// </summary>
public sealed record ManagementOfChangeDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid MocId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site where the change is proposed.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>MOC reference number (human-readable).</summary>
    public required string MocNumber { get; init; }

    /// <summary>Title describing the change. Max 200 characters.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed description of the proposed change. Max 4000 characters.</summary>
    public required string ChangeDescription { get; init; }

    /// <summary>Change type (e.g., Permanent, Temporary, Emergency).</summary>
    public required string ChangeType { get; init; }

    /// <summary>Risk level assigned (e.g., Low, Medium, High, Critical).</summary>
    public required string RiskLevel { get; init; }

    /// <summary>Worker who initiated the change request.</summary>
    public required Guid InitiatedByActorId { get; init; }

    /// <summary>Workflow status (e.g., Draft, UnderReview, Approved, Implemented, Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the change was approved, if applicable.</summary>
    public DateTimeOffset? ApprovedAtUtc { get; init; }

    /// <summary>UTC timestamp when the change was implemented, if applicable.</summary>
    public DateTimeOffset? ImplementedAtUtc { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}
