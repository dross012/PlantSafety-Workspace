// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Auditing.AuditReport
//  Layer:          Domain
//  File:           AuditReportGenerationRules.cs
//  Created:        2026-08-02
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Defines the required sections and permitted lifecycle transitions for
//                  AuditReport generation. Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.Auditing.AuditReport.Enums;

namespace PlantSafety.Domain.Auditing.AuditReport.Rules;

/// <summary>Defines the required sections and permitted lifecycle transitions for AuditReport generation.</summary>
public static class AuditReportGenerationRules
{
    /// <summary>
    /// The minimum set of sections every compliance-formatted report must include per
    /// Handbook Phase 5 §6.2: entity identity, timeline, approvals, exceptions, signatures.
    /// </summary>
    public static readonly IReadOnlyList<AuditReportSectionType> RequiredSections = AuditReportSectionType.All;

    /// <summary>Returns <see langword="true"/> when the supplied section set satisfies <see cref="RequiredSections"/>.</summary>
    public static bool SatisfiesRequiredSections(IEnumerable<AuditReportSectionType> sectionTypes)
        => RequiredSections.All(required => sectionTypes.Contains(required));

    /// <summary>Returns <see langword="true"/> when the report may transition to Generated from <paramref name="current"/>.</summary>
    public static bool CanMarkGenerated(AuditReportStatus current) => current == AuditReportStatus.Draft;

    /// <summary>Returns <see langword="true"/> when the report may transition to Failed from <paramref name="current"/>.</summary>
    public static bool CanMarkFailed(AuditReportStatus current) => current == AuditReportStatus.Draft;
}
