// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Document Management module.
//   Rewired 2026-08-03 onto the real Integrations.Documents read-model
//   (PlantSafety.Application.Integrations.Documents.Models.DocumentsItemDto)
//   exposed at v1/integrations/documents/list and v1/integrations/documents/{id}.
//   The originally-invented "Title/Description/Version/FileType/FileSizeBytes/
//   CreatedAtUtc/ExpiresAtUtc/OwnerName" shape had no analog anywhere in the real
//   Integrations.Documents domain (a synced-document-record read-model, not a
//   document-authoring aggregate) and has been replaced with the fields the real
//   query actually returns. See docs/Phase5/ui-gaps-scoping.md, DocumentManagement.
// SchemaVersion: 2026.2
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.DocumentManagement.Models;

/// <summary>
/// List/detail-view DTO for a document synced from an external system, matching
/// PlantSafety.Application.Integrations.Documents.Models.DocumentsItemDto. The real
/// Integrations.Documents read-model has no separate "list" vs "detail" shape, so the same DTO
/// backs both the Documents List and Document Detail pages.
/// </summary>
public sealed class DocumentListItemDto
{
    /// <summary>Gets the synced document record identifier.</summary>
    public Guid DocumentId { get; init; }

    /// <summary>Gets the site identifier the document is scoped to, or null when tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the external document management system the document was synced from.</summary>
    public string SourceSystem { get; init; } = string.Empty;

    /// <summary>Gets the name of the synced document file.</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>Gets the document type / category (e.g. Policy, Procedure, Training, Compliance).</summary>
    public string DocumentType { get; init; } = string.Empty;

    /// <summary>Gets the current sync status of the document (e.g. Synced, Pending, Error).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp this document was last synced.</summary>
    public DateTimeOffset SyncedAtUtc { get; init; }

    /// <summary>Gets the identity of the integration account or user that performed the sync.</summary>
    public Guid SyncedBy { get; init; }
}

/// <summary>
/// Full document detail DTO. The real Integrations.Documents read-model stores no additional
/// fields beyond the list shape, so this is a direct alias of <see cref="DocumentListItemDto"/>'s
/// fields, kept as its own type for API-contract clarity and future divergence.
/// </summary>
public sealed class DocumentDetailDto
{
    /// <summary>Gets the synced document record identifier.</summary>
    public Guid DocumentId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier the document is scoped to, or null when tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the external document management system the document was synced from.</summary>
    public string SourceSystem { get; init; } = string.Empty;

    /// <summary>Gets the name of the synced document file.</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>Gets the document type / category.</summary>
    public string DocumentType { get; init; } = string.Empty;

    /// <summary>Gets the current sync status of the document.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp this document was last synced.</summary>
    public DateTimeOffset SyncedAtUtc { get; init; }

    /// <summary>Gets the identity of the integration account or user that performed the sync.</summary>
    public Guid SyncedBy { get; init; }
}
