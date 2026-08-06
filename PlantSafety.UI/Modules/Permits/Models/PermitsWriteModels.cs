// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-05
// FILE DESCRIPTION: Client-side DTOs for the Compliance.Permits write/lifecycle
//   surface (api/v1/compliance/permits) — Create -> Approve/Reject ->
//   Complete/Expire -> Close -> Archive, plus a real by-id GET. This is a
//   distinct, generic Permit-to-Work aggregate from the read-only Operational.
//   PermitsToWork read-model backing PermitsListPage/PermitDetailPage — those
//   have no write commands; this is where CreatePermitsCommand and its
//   lifecycle actions actually live. Mirrors the shapes in
//   PlantSafety.Api.Compliance.Permits.Contracts.Requests.PermitsRequests and
//   PlantSafety.Application.Compliance.Permits.Models/Commands.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.Permits.Models;

/// <summary>Full read-model record for a single generic Permit (mirrors <c>PermitsItemDto</c>), backing GET v1/compliance/permits/{id}.</summary>
public sealed record PermitsItemDto
{
    /// <summary>Record identifier.</summary>
    public Guid PermitsId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived/Rejected/Expired).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Permit classification (e.g., HotWork, ConfinedSpace, Excavation, LOTO).</summary>
    public string PermitType { get; init; } = string.Empty;

    /// <summary>Human-readable permit number/reference.</summary>
    public string PermitNumber { get; init; } = string.Empty;

    /// <summary>Description of the work covered by this permit.</summary>
    public string WorkDescription { get; init; } = string.Empty;

    /// <summary>Physical location the permit applies to.</summary>
    public string Location { get; init; } = string.Empty;

    /// <summary>Hazards identified for this permitted work.</summary>
    public string HazardsIdentified { get; init; } = string.Empty;

    /// <summary>Identity of the user who requested the permit.</summary>
    public Guid? RequestedByUserId { get; init; }

    /// <summary>Identity of the user who approved/issued the permit, if issued.</summary>
    public Guid? ApprovedByUserId { get; init; }

    /// <summary>UTC timestamp the permit was issued.</summary>
    public DateTime? IssuedAtUtc { get; init; }

    /// <summary>UTC timestamp the permit expires.</summary>
    public DateTime? ExpiresAtUtc { get; init; }

    /// <summary>Reason recorded when a permit request is rejected.</summary>
    public string? RejectedReason { get; init; }

    /// <summary>UTC instant the permit request was rejected.</summary>
    public DateTime? RejectedAtUtc { get; init; }

    /// <summary>UTC instant the permitted work was marked completed.</summary>
    public DateTime? CompletedAtUtc { get; init; }

    /// <summary>UTC instant an issued permit expired without being completed/closed.</summary>
    public DateTime? ExpiredAtUtc { get; init; }

    /// <summary>UTC instant the permit was closed.</summary>
    public DateTime? ClosedAtUtc { get; init; }

    /// <summary>UTC instant the permit was archived for compliance retention.</summary>
    public DateTime? ArchivedAtUtc { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public Guid CreatedBy { get; init; }

    /// <summary>Optimistic-concurrency token. Must be echoed back on subsequent mutating commands.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for submitting a new generic Permit (POST v1/compliance/permits). Mirrors <c>CreatePermitsRequest</c>.</summary>
public sealed class CreatePermitsRequest
{
    /// <summary>Gets or sets the optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Gets or sets the permit classification (e.g., HotWork, ConfinedSpace, Excavation, LOTO).</summary>
    [Required]
    [StringLength(400)]
    public string PermitType { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable permit number/reference.</summary>
    [Required]
    [StringLength(400)]
    public string PermitNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of the work covered by this permit.</summary>
    [Required]
    [StringLength(2000)]
    public string WorkDescription { get; set; } = string.Empty;

    /// <summary>Gets or sets the physical location the permit applies to.</summary>
    [Required]
    [StringLength(400)]
    public string Location { get; set; } = string.Empty;

    /// <summary>Gets or sets the hazards identified for this permitted work.</summary>
    [Required]
    [StringLength(2000)]
    public string HazardsIdentified { get; set; } = string.Empty;
}

/// <summary>Result of submitting a new generic Permit. Mirrors <c>CreatePermitsResult</c>.</summary>
public sealed record CreatePermitsResultDto(Guid PermitsId, string Status);

/// <summary>Result payload shared by the Permit lifecycle-transition commands (Approve/Reject/Complete/Expire/Close/Archive). Mirrors <c>PermitsStatusResult</c>.</summary>
public sealed record PermitsStatusResultDto(Guid PermitsId, string Status);

/// <summary>Request body carrying only an optimistic-concurrency ETag (complete/expire/close/archive).</summary>
public sealed class EtagRequest
{
    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for approving and issuing a Permit request (POST .../approve). Mirrors <c>ApprovePermitsRequest</c>.</summary>
public sealed class ApprovePermitsRequest
{
    /// <summary>Gets or sets the optional validity window, in hours, from approval. Null means no computed expiration.</summary>
    [Range(1, 8760)]
    public int? ValidityHours { get; set; }

    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}

/// <summary>Request body for declining a Permit request (POST .../reject). Mirrors <c>RejectPermitsRequest</c>.</summary>
public sealed class RejectPermitsRequest
{
    /// <summary>Gets or sets the reason for rejection.</summary>
    [Required]
    [StringLength(2000)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}
