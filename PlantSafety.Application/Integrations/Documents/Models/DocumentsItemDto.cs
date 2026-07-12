// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO for a single document exchanged with an external system.
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.Documents.Models;

/// <summary>A single document record synced between PlantSafety and an external system (e.g. SharePoint/DMS integrations).</summary>
public sealed record DocumentsItemDto
{
    /// <summary>Synced document record identifier.</summary>
    public required Guid DocumentId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>External document management system the document was synced from.</summary>
    public required string SourceSystem { get; init; }

    /// <summary>Name of the synced document file.</summary>
    public required string FileName { get; init; }

    /// <summary>Classification of the synced document (e.g., PDF/Policy/Certificate).</summary>
    public required string DocumentType { get; init; }

    /// <summary>Current sync status of the document (e.g., Synced/Pending/Error).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the document was last synced.</summary>
    public required DateTime SyncedAtUtc { get; init; }

    /// <summary>Identity of the integration account or user that performed the sync.</summary>
    public required Guid SyncedBy { get; init; }
}
