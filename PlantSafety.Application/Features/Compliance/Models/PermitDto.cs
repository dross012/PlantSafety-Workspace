// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a Permit to Work (PTW) record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Compliance.Models;

/// <summary>
/// Read-side projection of a Permit aggregate.
/// Permits govern high-risk activities that require formal authorisation before work begins.
/// </summary>
public sealed record PermitDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid PermitId { get; init; }

    /// <summary>Tenant owning this permit.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site where the work is authorised.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Permit number (human-readable reference).</summary>
    public required string PermitNumber { get; init; }

    /// <summary>Type of permit (e.g., HotWork, ColdWork, ConfinedSpace, Electrical, LockoutTagout).</summary>
    public required string PermitType { get; init; }

    /// <summary>Description of work to be performed. Max 2000 characters.</summary>
    public required string WorkDescription { get; init; }

    /// <summary>Physical location where work will occur.</summary>
    public required string WorkLocation { get; init; }

    /// <summary>Worker or crew lead performing the work.</summary>
    public required Guid WorkLeadId { get; init; }

    /// <summary>Approver who authorised the permit.</summary>
    public Guid? ApprovedByActorId { get; init; }

    /// <summary>UTC timestamp when the permit was approved, if applicable.</summary>
    public DateTimeOffset? ApprovedAtUtc { get; init; }

    /// <summary>UTC timestamp when the permit becomes valid.</summary>
    public required DateTimeOffset ValidFromUtc { get; init; }

    /// <summary>UTC timestamp when the permit expires.</summary>
    public required DateTimeOffset ValidToUtc { get; init; }

    /// <summary>Workflow status (e.g., Draft, PendingApproval, Active, Suspended, Closed, Expired).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }

    /// <summary>Row version for concurrency.</summary>
    public required string ETag { get; init; }
}
