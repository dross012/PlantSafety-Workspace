// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-21
// FILE DESCRIPTION: Request DTO for creating a new SafetyPolicy. Domain fields only;
//   envelope fields (CommandId/CorrelationId/TenantId/etc.) are populated server-side by
//   RequestEnvelopeFactory.
// SchemaVersion: 1

namespace PlantSafety.Api.SafetyPolicies.Contracts.Requests;

/// <summary>Request body for <c>POST /api/v1/safetypolicies</c>.</summary>
public sealed class CreateSafetyPolicyRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Human-readable title for the new policy record.</summary>
    public required string SafetyPolicyTitle { get; init; }

    /// <summary>PolicyCategory code governing this record.</summary>
    public required string Category { get; init; }

    /// <summary>Identity of the accountable owner.</summary>
    public required Guid OwnerId { get; init; }

    /// <summary>Optional UTC timestamp the record becomes effective.</summary>
    public DateTime? EffectiveAtUtc { get; init; }

    /// <summary>Whether formal review/acknowledgment is required.</summary>
    public bool ReviewRequired { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/safetypolicies/{safetyPolicyId}/reject</c>.</summary>
public sealed class RejectSafetyPolicyRequest
{
    /// <summary>Reason the policy is being rejected.</summary>
    public required string Reason { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/safetypolicies/{safetyPolicyId}/publish</c>.</summary>
public sealed class PublishSafetyPolicyRequest
{
    /// <summary>UTC instant the policy becomes effective.</summary>
    public required DateTime EffectiveAtUtc { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/safetypolicies/{safetyPolicyId}/complete-review</c>.</summary>
public sealed class CompleteSafetyPolicyReviewRequest
{
    /// <summary>Optional new review cadence to apply going forward.</summary>
    public string? NewReviewCadence { get; init; }
}

/// <summary>Request body for <c>POST /api/v1/safetypolicies/{safetyPolicyId}/retire</c>.</summary>
public sealed class RetireSafetyPolicyRequest
{
    /// <summary>Reason the policy is being retired.</summary>
    public required string Reason { get; init; }
}
