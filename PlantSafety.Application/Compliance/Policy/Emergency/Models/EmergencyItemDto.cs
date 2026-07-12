// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Emergency record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Policy.Emergency.Models;

/// <summary>A single Emergency read-model item.</summary>
public sealed record EmergencyItemDto
{
    /// <summary>Policy Id identifier.</summary>
    public required Guid PolicyId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Emergency Type.</summary>
    public required string EmergencyType { get; init; }

    /// <summary>Response Procedure Ref.</summary>
    public required string ResponseProcedureRef { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Effective At Utc.</summary>
    public required DateTime EffectiveAtUtc { get; init; }

    /// <summary>Owner Id.</summary>
    public required Guid OwnerId { get; init; }
}
