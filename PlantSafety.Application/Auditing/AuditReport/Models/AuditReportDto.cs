// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: API/read-model DTO for a full AuditReport aggregate.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.AuditReport.Models;

/// <summary>Read-model DTO for a full AuditReport.</summary>
public sealed record AuditReportDto
{
    /// <summary>Report identity.</summary>
    public required Guid AuditReportId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Type discriminator of the documented entity.</summary>
    public required string TargetEntityType { get; init; }

    /// <summary>Identity of the documented entity.</summary>
    public required Guid TargetEntityId { get; init; }

    /// <summary>Output format code (Pdf/Csv/Json).</summary>
    public required string Format { get; init; }

    /// <summary>Lifecycle status code (Draft/Generated/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>Composed sections, in canonical print order.</summary>
    public required IReadOnlyList<AuditReportSectionDto> Sections { get; init; }

    /// <summary>Rendered content size in bytes, once generated.</summary>
    public long? ContentSizeBytes { get; init; }

    /// <summary>Storage reference for the rendered output, once generated.</summary>
    public string? RenderedContentReference { get; init; }

    /// <summary>Reason generation failed, when Status is Failed.</summary>
    public string? FailureReason { get; init; }

    /// <summary>Identity of the user who requested this report.</summary>
    public required Guid RequestedBy { get; init; }

    /// <summary>UTC instant the report was requested.</summary>
    public required DateTime RequestedAtUtc { get; init; }

    /// <summary>UTC instant generation completed, if any.</summary>
    public DateTime? CompletedAtUtc { get; init; }
}
