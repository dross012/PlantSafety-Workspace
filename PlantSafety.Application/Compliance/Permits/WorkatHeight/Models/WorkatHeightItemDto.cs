// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single work-at-height permit record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Permits.WorkatHeight.Models;

/// <summary>A single work-at-height permit record.</summary>
public sealed record WorkatHeightItemDto
{
    /// <summary>Permit identifier.</summary>
    public required Guid PermitId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Physical location where the work is being performed.</summary>
    public required string WorkLocation { get; init; }

    /// <summary>Type of fall protection system in use (e.g., Harness, Guardrail, SafetyNet).</summary>
    public required string FallProtectionType { get; init; }

    /// <summary>Current permit status (e.g., Active/Expired/Closed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the permit was issued.</summary>
    public required DateTime IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp when the permit expires.</summary>
    public required DateTime ExpiresAtUtc { get; init; }

    /// <summary>Identity of the user who approved the permit.</summary>
    public required Guid ApprovedBy { get; init; }
}
