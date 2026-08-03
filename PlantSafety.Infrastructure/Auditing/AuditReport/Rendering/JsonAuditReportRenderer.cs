// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Infrastructure
//  File:           JsonAuditReportRenderer.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code infrastructure-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    IAuditReportRenderer implementation producing a structured JSON export of the
//                  composed AuditReport -- also serves as the "snapshot baseline" export format
//                  (Handbook Phase 5 §6.2): a complete, point-in-time, machine-readable capture of
//                  the report's sections. Additive-only evolution.
// ============================================================================
using System.Text.Json;
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Application.Auditing.AuditReport.Abstractions;

namespace PlantSafety.Infrastructure.Auditing.AuditReport.Rendering;

/// <summary>Renders a composed AuditReport to a structured JSON export.</summary>
public sealed class JsonAuditReportRenderer : IAuditReportRenderer
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <inheritdoc/>
    public AuditReportFormat Format => AuditReportFormat.Json;

    /// <inheritdoc/>
    public Task<byte[]> RenderAsync(AuditReportRenderModel model, CancellationToken cancellationToken)
    {
        var payload = new
        {
            model.AuditReportId,
            model.TargetEntityType,
            model.TargetEntityId,
            model.GeneratedAtUtc,
            Sections = model.Sections.Select(s => new { s.SectionCode, s.Heading, Content = JsonSerializer.Deserialize<JsonElement?>(s.ContentJson) }),
        };

        return Task.FromResult(JsonSerializer.SerializeToUtf8Bytes(payload, Options));
    }
}
