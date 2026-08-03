// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Auditing module. Backed by the
//   PlantSafety.Api Compliance Auditing ComplianceAudit read-model
//   (api/v1/compliance/auditing/complianceaudit) — the general Auditing
//   aggregate record ("A single compliance audit record, backed by the
//   Auditing aggregate" per the Application-layer XML doc) — which wraps its
//   payload in the platform-wide ApiResponse<T> shape
//   (Success/Data/Errors/Metadata). ReadModelEnvelopeDto/ReadModelPayloadDto
//   mirror that wire shape exactly so SafetyApiClientBase.GetAsync<T> can
//   deserialize it directly. No domain logic present — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Auditing.Models;

/// <summary>A single compliance audit record, backed by the Auditing aggregate.</summary>
public sealed record AuditItemDto
{
    /// <summary>Identifier of the Auditing aggregate this record represents.</summary>
    public Guid AuditingId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Audit type/standard (e.g., ISO45001, OSHA, Internal).</summary>
    public string AuditType { get; init; } = string.Empty;

    /// <summary>Current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the audit was conducted.</summary>
    public DateTime ConductedAtUtc { get; init; }

    /// <summary>Identity of the user who conducted the audit.</summary>
    public Guid ConductedBy { get; init; }
}

/// <summary>Read-model payload envelope, matching PlantSafety.Api's per-endpoint Response records.</summary>
/// <typeparam name="TItem">The item type contained in <see cref="Items"/>.</typeparam>
public sealed record ReadModelPayloadDto<TItem>
{
    /// <summary>Tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Result items. Never null; empty when no results.</summary>
    public IReadOnlyList<TItem> Items { get; init; } = [];
}

/// <summary>
/// Client-side mirror of PlantSafety.Api.Models.ApiResponse&lt;T&gt; — the platform-wide
/// controller envelope (Success/Data/Errors/Metadata) used by all Compliance/Operational/
/// CrossDomain/Analytics read-model endpoints.
/// </summary>
/// <typeparam name="TItem">The item type contained in the payload's <c>Items</c> list.</typeparam>
public sealed record ReadModelEnvelopeDto<TItem>
{
    /// <summary>Gets a value indicating whether the API operation succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Gets the payload. Null when <see cref="Success"/> is false.</summary>
    public ReadModelPayloadDto<TItem>? Data { get; init; }

    /// <summary>Gets human-readable error messages. Empty on success.</summary>
    public IReadOnlyList<string> Errors { get; init; } = [];
}

// ============================================================
// Phase 5 §6.1 — AuditTimeline (PlantSafety.Application.Auditing.AuditTimeline.*)
// ============================================================

/// <summary>Read-model DTO for a single normalized AuditTimeline entry (mirrors <c>AuditTimelineEntryDto</c>).</summary>
public sealed record AuditTimelineEntryDto
{
    /// <summary>Entry identity.</summary>
    public Guid AuditTimelineEntryId { get; init; }

    /// <summary>Entry type code (EventRaised/StateChange/ConcurrencyConflict/SchedulingDecision/ComplianceViolation).</summary>
    public string EntryType { get; init; } = string.Empty;

    /// <summary>Type discriminator of the entity this entry belongs to.</summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>Identity of the entity this entry belongs to.</summary>
    public Guid EntityId { get; init; }

    /// <summary>Short, human-readable summary of the entry.</summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>Optional structured detail payload (JSON).</summary>
    public string? DetailJson { get; init; }

    /// <summary>Identity of the associated actor, if known.</summary>
    public Guid? ActorId { get; init; }

    /// <summary>UTC instant this entry occurred; the ordering key.</summary>
    public DateTime UtcTimestamp { get; init; }

    /// <summary>Correlation identifier of the originating operation, if known.</summary>
    public Guid? CorrelationId { get; init; }
}

/// <summary>Read-model DTO for a fully composed, chronologically-ordered AuditTimeline (mirrors <c>AuditTimelineDto</c>).</summary>
public sealed record AuditTimelineDto
{
    /// <summary>Type discriminator of the entity this timeline belongs to.</summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>Identity of the entity this timeline belongs to.</summary>
    public Guid EntityId { get; init; }

    /// <summary>All entries, strictly ordered by UtcTimestamp.</summary>
    public IReadOnlyList<AuditTimelineEntryDto> Entries { get; init; } = [];

    /// <summary>UTC instant this timeline was composed.</summary>
    public DateTime ComposedAtUtc { get; init; }
}

// ============================================================
// Phase 5 §6.2 — AuditReport (PlantSafety.Application.Auditing.AuditReport.*)
// ============================================================

/// <summary>Read-model DTO for a single compliance-formatted AuditReport section (mirrors <c>AuditReportSectionDto</c>).</summary>
public sealed record AuditReportSectionDto
{
    /// <summary>Canonical section type code.</summary>
    public string SectionType { get; init; } = string.Empty;

    /// <summary>Human-readable heading.</summary>
    public string Heading { get; init; } = string.Empty;

    /// <summary>Structured content payload (JSON).</summary>
    public string ContentJson { get; init; } = string.Empty;
}

/// <summary>Read-model DTO for a full AuditReport (mirrors <c>AuditReportDto</c>).</summary>
public sealed record AuditReportDto
{
    /// <summary>Report identity.</summary>
    public Guid AuditReportId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type discriminator of the documented entity.</summary>
    public string TargetEntityType { get; init; } = string.Empty;

    /// <summary>Identity of the documented entity.</summary>
    public Guid TargetEntityId { get; init; }

    /// <summary>Output format code (Pdf/Csv/Json).</summary>
    public string Format { get; init; } = string.Empty;

    /// <summary>Lifecycle status code (Draft/Generated/Failed).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Composed sections, in canonical print order.</summary>
    public IReadOnlyList<AuditReportSectionDto> Sections { get; init; } = [];

    /// <summary>Rendered content size in bytes, once generated.</summary>
    public long? ContentSizeBytes { get; init; }

    /// <summary>Storage reference for the rendered output, once generated.</summary>
    public string? RenderedContentReference { get; init; }

    /// <summary>Reason generation failed, when Status is Failed.</summary>
    public string? FailureReason { get; init; }

    /// <summary>Identity of the user who requested this report.</summary>
    public Guid RequestedBy { get; init; }

    /// <summary>UTC instant the report was requested.</summary>
    public DateTime RequestedAtUtc { get; init; }

    /// <summary>UTC instant generation completed, if any.</summary>
    public DateTime? CompletedAtUtc { get; init; }
}

/// <summary>Rendered content payload returned by the AuditReport download endpoint (mirrors <c>AuditReportContentResult</c>).</summary>
public sealed record AuditReportContentDto
{
    /// <summary>MIME content type of <see cref="Content"/>.</summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>Suggested download file name.</summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>Raw rendered bytes (base64-encoded on the wire; deserializes directly to <c>byte[]</c>).</summary>
    public byte[] Content { get; init; } = [];
}

/// <summary>Request body for <c>GenerateAuditReportCommand</c>'s caller-supplied fields.</summary>
public sealed record GenerateAuditReportRequest
{
    /// <summary>Optional site scope.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type discriminator of the entity being documented.</summary>
    public string TargetEntityType { get; init; } = string.Empty;

    /// <summary>Identity of the entity being documented.</summary>
    public Guid TargetEntityId { get; init; }

    /// <summary>Requested output format code ("Pdf", "Csv", or "Json").</summary>
    public string Format { get; init; } = "Pdf";

    /// <summary>Pre-serialized JSON for the Entity Identity section.</summary>
    public string EntityIdentityJson { get; init; } = "{}";

    /// <summary>Pre-serialized JSON for the Approvals section.</summary>
    public string ApprovalsJson { get; init; } = "{}";

    /// <summary>Pre-serialized JSON for the Exceptions section.</summary>
    public string ExceptionsJson { get; init; } = "{}";

    /// <summary>Pre-serialized JSON for the Signatures/Attestations section.</summary>
    public string SignaturesJson { get; init; } = "{}";
}
