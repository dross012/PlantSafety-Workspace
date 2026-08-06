// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-05
// FILE DESCRIPTION: Client-side DTOs for the Compliance.Policy write/lifecycle
//   surface (api/v1/compliance/policy) — Create -> Approve -> RecordAcknowledgment
//   (repeatable) -> Supersede -> Close -> Archive, plus a real by-id GET. This is
//   a distinct, generic Policy aggregate from the read-only Safety Policies/SOPs
//   read-models backing PoliciesListPage/PolicyDetailPage/ProcedureDetailPage —
//   those have no write commands; this is where CreatePolicyCommand and its
//   lifecycle actions actually live. Mirrors the shapes in
//   PlantSafety.Api.Compliance.Policy.Contracts.Requests.PolicyRequests and
//   PlantSafety.Application.Compliance.Policy.Models/Commands.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace PlantSafety.UI.Modules.Policies.Models;

/// <summary>Full read-model record for a single generic Policy (mirrors <c>PolicyRecordItemDto</c>), backing GET v1/compliance/policy/{id}.</summary>
public sealed record PolicyRecordItemDto
{
    /// <summary>Record identifier.</summary>
    public Guid PolicyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Human-readable policy title.</summary>
    public string PolicyTitle { get; init; } = string.Empty;

    /// <summary>Monotonically increasing policy revision number, starting at 1.</summary>
    public int PolicyVersion { get; init; }

    /// <summary>Policy category (e.g., PPE, HotWork, EmergencyResponse, GeneralSafety).</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Identity of the accountable policy owner.</summary>
    public Guid? OwnerId { get; init; }

    /// <summary>UTC timestamp the policy becomes effective.</summary>
    public DateTime? EffectiveAtUtc { get; init; }

    /// <summary>Indicates whether personnel acknowledgment is required for this policy.</summary>
    public bool AcknowledgmentRequired { get; init; }

    /// <summary>Count of personnel who have acknowledged this policy.</summary>
    public int AcknowledgmentCount { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public Guid CreatedBy { get; init; }

    /// <summary>Optimistic-concurrency token. Must be echoed back on subsequent mutating commands.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for submitting a new generic Policy document (POST v1/compliance/policy). Mirrors <c>CreatePolicyRequest</c>.</summary>
public sealed class CreatePolicyRecordRequest
{
    /// <summary>Gets or sets the optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; set; }

    /// <summary>Gets or sets the human-readable policy title.</summary>
    [Required]
    [StringLength(400)]
    public string PolicyTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the policy category (e.g., PPE, HotWork, EmergencyResponse, GeneralSafety).</summary>
    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether personnel acknowledgment is required for this policy.</summary>
    public bool AcknowledgmentRequired { get; set; } = true;
}

/// <summary>Result of submitting a new generic Policy document. Mirrors <c>CreatePolicyResult</c>.</summary>
public sealed record CreatePolicyResultDto(Guid PolicyId, string Status);

/// <summary>Result payload shared by the Policy lifecycle-transition commands (Approve/RecordAcknowledgment/Supersede/Close/Archive). Mirrors <c>PolicyStatusResult</c>.</summary>
public sealed record PolicyStatusResultDto(Guid PolicyId, string Status, int AcknowledgmentCount);

/// <summary>Request body carrying only an optimistic-concurrency ETag (approve/acknowledge/supersede/close/archive).</summary>
public sealed class EtagRequest
{
    /// <summary>Gets or sets the optimistic-concurrency token from the last read of this record. Null skips the check.</summary>
    public string? ETag { get; set; }
}
