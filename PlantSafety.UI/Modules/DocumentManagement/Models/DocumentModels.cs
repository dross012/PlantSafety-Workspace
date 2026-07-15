// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Document Management module.
//   Covers the Documents List and Document Detail pages using the generic
//   list/detail template (no dedicated architecture manual for this module).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.DocumentManagement.Models;

/// <summary>Lightweight list-view DTO for a document.</summary>
public sealed class DocumentListItemDto
{
    /// <summary>Gets the document identifier (UUIDv7).</summary>
    public Guid DocumentId { get; init; }

    /// <summary>Gets the document title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the document category (e.g. Policy, Procedure, Training, Compliance).</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the document status (Draft, UnderReview, Published, Archived, Expired).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the current version label.</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp this document was last updated.</summary>
    public DateTimeOffset LastUpdatedUtc { get; init; }

    /// <summary>Gets the owner display name.</summary>
    public string OwnerName { get; init; } = string.Empty;
}

/// <summary>Full document detail DTO.</summary>
public sealed class DocumentDetailDto
{
    /// <summary>Gets the document identifier.</summary>
    public Guid DocumentId { get; init; }

    /// <summary>Gets the document title.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Gets the document description/summary.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the document category.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the document status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the current version label.</summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>Gets the file type/extension (e.g. PDF, DOCX).</summary>
    public string FileType { get; init; } = string.Empty;

    /// <summary>Gets the file size in bytes.</summary>
    public long FileSizeBytes { get; init; }

    /// <summary>Gets the UTC timestamp this document was created.</summary>
    public DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp this document was last updated.</summary>
    public DateTimeOffset LastUpdatedUtc { get; init; }

    /// <summary>Gets the UTC timestamp this document expires, if any.</summary>
    public DateTimeOffset? ExpiresAtUtc { get; init; }

    /// <summary>Gets the owner display name.</summary>
    public string OwnerName { get; init; } = string.Empty;
}
