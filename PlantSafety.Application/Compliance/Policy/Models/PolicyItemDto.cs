// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Policy
//  Layer:          Application
//  File:           PolicyItemDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single Policy record. Backs
//                  GetPolicyByIdQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Models.PermitsItemDto's shape.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Policy.Models;

/// <summary>A single Policy read-model record.</summary>
public sealed record PolicyItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PolicyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Human-readable policy title.</summary>
    public required string PolicyTitle { get; init; }

    /// <summary>Monotonically increasing policy revision number, starting at 1.</summary>
    public required int PolicyVersion { get; init; }

    /// <summary>Policy category (e.g., PPE, HotWork, EmergencyResponse, GeneralSafety).</summary>
    public required string Category { get; init; }

    /// <summary>Identity of the accountable policy owner.</summary>
    public Guid? OwnerId { get; init; }

    /// <summary>UTC timestamp the policy becomes effective.</summary>
    public DateTime? EffectiveAtUtc { get; init; }

    /// <summary>Indicates whether personnel acknowledgment is required for this policy.</summary>
    public required bool AcknowledgmentRequired { get; init; }

    /// <summary>Count of personnel who have acknowledged this policy.</summary>
    public required int AcknowledgmentCount { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>Identity of the user who created this record.</summary>
    public required Guid CreatedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}
