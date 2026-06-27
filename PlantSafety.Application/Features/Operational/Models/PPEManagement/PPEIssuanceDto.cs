// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side DTO for a PPE Issuance record.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Operational.Models.PPEManagement;

/// <summary>
/// Read-side projection of a PPEIssuance record.
/// Tracks the issuance of Personal Protective Equipment to workers.
/// </summary>
public sealed record PPEIssuanceDto
{
    /// <summary>Unique identifier (UUIDv7).</summary>
    public required Guid IssuanceId { get; init; }

    /// <summary>Tenant owning this record.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site where PPE was issued.</summary>
    public required Guid SiteId { get; init; }

    /// <summary>Worker who received the PPE.</summary>
    public required Guid WorkerId { get; init; }

    /// <summary>PPE item type (e.g., HardHat, SafetyGlasses, HighVisBest, GlovesCut5, SafetyBoots).</summary>
    public required string PPEType { get; init; }

    /// <summary>PPE item size or specification.</summary>
    public string? Specification { get; init; }

    /// <summary>Quantity issued.</summary>
    public required int Quantity { get; init; }

    /// <summary>UTC timestamp when PPE was issued.</summary>
    public required DateTimeOffset IssuedAtUtc { get; init; }

    /// <summary>Worker or supervisor who issued the PPE.</summary>
    public required Guid IssuedByActorId { get; init; }

    /// <summary>UTC timestamp when PPE is expected to expire or be replaced.</summary>
    public DateTimeOffset? ExpiresAtUtc { get; init; }

    /// <summary>UTC timestamp of last modification.</summary>
    public required DateTimeOffset LastModifiedAtUtc { get; init; }
}
