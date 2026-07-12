// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single crisis management event.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Models;

/// <summary>A single declared crisis event tracked by the CrisisManagement module.</summary>
public sealed record CrisisManagementItemDto
{
    /// <summary>Crisis event identifier.</summary>
    public required Guid CrisisId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Short crisis title.</summary>
    public required string Title { get; init; }

    /// <summary>Severity classification (e.g., Low/Medium/High/Critical).</summary>
    public required string Severity { get; init; }

    /// <summary>Current crisis status (e.g., Declared/Active/Stabilized/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the crisis was declared.</summary>
    public required DateTime DeclaredAtUtc { get; init; }

    /// <summary>Identity of the user who declared the crisis.</summary>
    public required Guid DeclaredBy { get; init; }
}
