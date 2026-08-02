// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Read-model item DTO for a single PermitExpiration record. Permit expiration tracking: issue/expiry windows and extension requests.
// SchemaVersion: 1

namespace PlantSafety.Application.PermitToWork.PermitExpiration.Models;

/// <summary>A single PermitExpiration read-model record.</summary>
public sealed record PermitExpirationItemDto
{
    /// <summary>Record identifier.</summary>
    public required Guid PermitExpirationId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current status (e.g., Created/Active/Completed/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>PermitNumber.</summary>
    public required  string PermitNumber { get; init; }

    /// <summary>IssuedAtUtc.</summary>
    public DateTime? IssuedAtUtc { get; init; }

    /// <summary>ExpiresAtUtc.</summary>
    public DateTime? ExpiresAtUtc { get; init; }

    /// <summary>ExtensionRequested.</summary>
    public required  bool ExtensionRequested { get; init; }

    /// <summary>UTC timestamp when the record was created.</summary>
    public required DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}