// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance
//  Layer:          Application
//  File:           StandardsComplianceReportMapper.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Maps the Domain-layer PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects
//                  .StandardsComplianceReport/StandardsComplianceFinding records (returned by every
//                  consensus-standards compliance engine under Compliance.Standards) onto the
//                  Application-layer StandardsComplianceReportDto/StandardsComplianceFindingDto
//                  shapes returned to API callers. Shared by every
//                  EvaluateXComplianceCommandHandler/EvaluateXStandardsCommandHandler under
//                  Compliance.Standards.Compliance so the translation logic is written once.
//                  Mirrors Compliance.EPA.Compliance.Models.EpaComplianceReportMapper's pattern
//                  exactly. Additive-only evolution.
// ============================================================================
using DomainStandardsComplianceReport = PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects.StandardsComplianceReport;
using DomainStandardsComplianceFinding = PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects.StandardsComplianceFinding;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Models;

/// <summary>Maps Domain-layer consensus-standards compliance-engine output onto Application-layer DTOs.</summary>
public static class StandardsComplianceReportMapper
{
    /// <summary>Maps a Domain <see cref="DomainStandardsComplianceReport"/> onto a <see cref="StandardsComplianceReportDto"/>.</summary>
    public static StandardsComplianceReportDto ToDto(DomainStandardsComplianceReport report) => new()
    {
        EngineCode = report.EngineCode,
        EvaluatedAtUtc = report.EvaluatedAtUtc.Value,
        Violations = report.Violations.Select(ToDto).ToList(),
        Warnings = report.Warnings.Select(ToDto).ToList(),
        RequiredActions = report.RequiredActions.ToList(),
        IsCompliant = report.IsCompliant
    };

    /// <summary>Maps a Domain <see cref="DomainStandardsComplianceFinding"/> onto a <see cref="StandardsComplianceFindingDto"/>.</summary>
    public static StandardsComplianceFindingDto ToDto(DomainStandardsComplianceFinding finding) => new()
    {
        Citation = finding.Citation,
        Description = finding.Description,
        Severity = finding.Severity,
        RelatedEntityId = finding.RelatedEntityId
    };
}
