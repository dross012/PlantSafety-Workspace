// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportSection.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity of AuditReport representing one compliance-formatted section
//                  (Entity Identity, Timeline, Approvals, Exceptions, Signatures) of the printed
//                  report. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditReport.Enums;
using PlantSafety.Domain.Auditing.AuditReport.ValueObjects.Ids;

namespace PlantSafety.Domain.Auditing.AuditReport.Entities;

/// <summary>A single compliance-formatted section within an AuditReport.</summary>
public sealed class AuditReportSection
{
    /// <summary>Strong-typed identity of this section.</summary>
    public AuditReportSectionId AuditReportSectionId { get; private set; }

    /// <summary>Owning report.</summary>
    public AuditReportId AuditReportId { get; private set; }

    /// <summary>Canonical section type.</summary>
    public AuditReportSectionType SectionType { get; private set; } = AuditReportSectionType.Default;

    /// <summary>Human-readable heading printed for this section.</summary>
    public string Heading { get; private set; } = string.Empty;

    /// <summary>Structured content payload for this section, serialized as JSON for renderer consumption.</summary>
    public string ContentJson { get; private set; } = "{}";

    // For rehydration only.
    private AuditReportSection() { }

    /// <summary>Creates a new AuditReportSection.</summary>
    public static AuditReportSection Create(AuditReportId reportId, AuditReportSectionType sectionType, string heading, string contentJson)
        => new()
        {
            AuditReportSectionId = AuditReportSectionId.New(),
            AuditReportId = reportId,
            SectionType = sectionType,
            Heading = (heading ?? string.Empty).Trim(),
            ContentJson = string.IsNullOrWhiteSpace(contentJson) ? "{}" : contentJson,
        };
}
