// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single lifting/crane operation permit record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Permits.Lifting.Models;

/// <summary>A single lifting or crane operation permit record.</summary>
public sealed record LiftingItemDto
{
    /// <summary>Permit identifier.</summary>
    public required Guid PermitId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Description of the load being lifted.</summary>
    public required string LoadDescription { get; init; }

    /// <summary>Weight of the load, in kilograms.</summary>
    public required decimal LoadWeightKg { get; init; }

    /// <summary>Current permit status (e.g., Active/Expired/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the permit was issued.</summary>
    public required DateTime IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp when the permit expires.</summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>Identity of the user who approved the permit.</summary>
    public required Guid ApprovedBy { get; init; }
}
