// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Clean rendering abstraction for printable/exportable audit reports (Handbook
//   Phase 5 §6.2). One implementation is registered per supported AuditReportFormat (Pdf, Csv,
//   Json); GenerateAuditReportCommandHandler selects the matching renderer at generation time.
// SchemaVersion: 1

using PlantSafety.Domain.Auditing.AuditReport.Enums;

namespace PlantSafety.Application.Auditing.AuditReport.Abstractions;

/// <summary>Renders a composed audit report to a concrete byte-stream output format.</summary>
public interface IAuditReportRenderer
{
    /// <summary>The output format this renderer produces.</summary>
    AuditReportFormat Format { get; }

    /// <summary>Renders the supplied report content model to the renderer's output format.</summary>
    Task<byte[]> RenderAsync(AuditReportRenderModel model, CancellationToken cancellationToken);
}

/// <summary>Compliance-formatted content handed to an <see cref="IAuditReportRenderer"/> for rendering.</summary>
/// <param name="AuditReportId">Identity of the report being rendered.</param>
/// <param name="TargetEntityType">Type discriminator of the documented entity.</param>
/// <param name="TargetEntityId">Identity of the documented entity.</param>
/// <param name="GeneratedAtUtc">UTC instant rendering was requested.</param>
/// <param name="Sections">Ordered compliance-formatted sections (Entity Identity, Timeline, Approvals, Exceptions, Signatures).</param>
public sealed record AuditReportRenderModel(
    Guid AuditReportId,
    string TargetEntityType,
    Guid TargetEntityId,
    DateTime GeneratedAtUtc,
    IReadOnlyList<AuditReportRenderSection> Sections);

/// <summary>A single section handed to the renderer, already resolved to its heading and JSON content.</summary>
/// <param name="SectionCode">Canonical section type code (EntityIdentity/Timeline/Approvals/Exceptions/Signatures).</param>
/// <param name="Heading">Human-readable heading.</param>
/// <param name="ContentJson">Structured content payload as JSON.</param>
public sealed record AuditReportRenderSection(string SectionCode, string Heading, string ContentJson);
