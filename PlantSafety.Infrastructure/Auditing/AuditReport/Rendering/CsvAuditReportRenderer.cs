// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Infrastructure
//  File:           CsvAuditReportRenderer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    IAuditReportRenderer implementation producing a flat CSV export of the
//                  composed AuditReport sections (one row per section, with the section's JSON
//                  content quoted). Uses only System.Text -- no external CSV library dependency,
//                  since the export shape is fixed and simple (Handbook §2.16 favors
//                  dependency-free deterministic code where practical). Additive-only evolution.
// ============================================================================
using System.Text;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Application.Auditing.AuditReport.Abstractions;

namespace PlantSafety.Infrastructure.Auditing.AuditReport.Rendering;

/// <summary>Renders a composed AuditReport to a flat CSV export.</summary>
public sealed class CsvAuditReportRenderer : IAuditReportRenderer
{
    /// <inheritdoc/>
    public AuditReportFormat Format => AuditReportFormat.Csv;

    /// <inheritdoc/>
    public Task<byte[]> RenderAsync(AuditReportRenderModel model, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("AuditReportId,TargetEntityType,TargetEntityId,GeneratedAtUtc,SectionCode,Heading,ContentJson");

        foreach (var section in model.Sections)
        {
            sb.Append(Quote(model.AuditReportId.ToString("D"))).Append(',')
              .Append(Quote(model.TargetEntityType)).Append(',')
              .Append(Quote(model.TargetEntityId.ToString("D"))).Append(',')
              .Append(Quote(model.GeneratedAtUtc.ToString("O"))).Append(',')
              .Append(Quote(section.SectionCode)).Append(',')
              .Append(Quote(section.Heading)).Append(',')
              .Append(Quote(section.ContentJson))
              .AppendLine();
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    private static string Quote(string value)
        => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
}
