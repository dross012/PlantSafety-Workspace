// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Documents record.
// SchemaVersion: 1

namespace PlantSafety.Application.Compliance.Policy.Documents.Models;

/// <summary>A single Documents read-model item.</summary>
public sealed record DocumentsItemDto
{
    /// <summary>Document Id identifier.</summary>
    public required Guid DocumentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Title.</summary>
    public required string Title { get; init; }

    /// <summary>Document Type.</summary>
    public required string DocumentType { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>Published At Utc.</summary>
    public required DateTime PublishedAtUtc { get; init; }

    /// <summary>Owner Id.</summary>
    public required Guid OwnerId { get; init; }
}
