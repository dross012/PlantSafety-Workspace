// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Infrastructure
//  File:           QuestPdfAuditReportRenderer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    IAuditReportRenderer implementation producing a printable PDF document via
//                  QuestPDF (new NuGet package -- see PlantSafety.Infrastructure.csproj; no PDF
//                  library was previously referenced anywhere in this solution, confirmed via
//                  solution-wide search). QuestPDF is a well-governed, actively maintained,
//                  pure-.NET fluent-API PDF generator with no reflection-based mapping, no
//                  dynamic SQL, and no ORM surface -- it does not fall under any Handbook §2.16
//                  prohibited-technology category. Licensed under the QuestPDF Community license
//                  (free for organizations/projects under the license's revenue threshold); flag
//                  for legal/licensing review before production use at scale. Renders the five
//                  canonical compliance-formatted sections (Entity Identity, Timeline, Approvals,
//                  Exceptions, Signatures) in print order. Additive-only evolution.
// ============================================================================
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Application.Auditing.AuditReport.Abstractions;

namespace PlantSafety.Infrastructure.Auditing.AuditReport.Rendering;

/// <summary>Renders a composed AuditReport to a printable PDF document via QuestPDF.</summary>
public sealed class QuestPdfAuditReportRenderer : IAuditReportRenderer
{
    static QuestPdfAuditReportRenderer()
    {
        // QuestPDF Community license: free for companies/individuals with < $1M USD annual gross
        // revenue and organizations that are not-for-profit/open-source. See
        // https://www.questpdf.com/license/ -- flagged for legal review, see class remarks.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc/>
    public AuditReportFormat Format => AuditReportFormat.Pdf;

    /// <inheritdoc/>
    public Task<byte[]> RenderAsync(AuditReportRenderModel model, CancellationToken cancellationToken)
    {
        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text($"Audit Report").FontSize(18).Bold();
                    col.Item().Text($"{model.TargetEntityType} — {model.TargetEntityId:D}").FontSize(11).FontColor(Colors.Grey.Darken2);
                    col.Item().Text($"Generated: {model.GeneratedAtUtc:O} UTC — Report {model.AuditReportId:D}").FontSize(9).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(4).LineHorizontal(1);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    foreach (var section in model.Sections)
                    {
                        col.Item().PaddingTop(12).Text(section.Heading).FontSize(13).Bold();
                        col.Item().PaddingTop(2).Text(section.ContentJson).FontSize(8).FontFamily("Consolas");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }
}
