// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance
//  Layer:          Application
//  File:           EpaComplianceReportMapper.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Maps the Domain-layer PlantSafety.Domain.Compliance.EPA.Shared.ValueObjects
//                  .EpaComplianceReport/EpaComplianceFinding records (returned by every EPA
//                  compliance engine) onto the Application-layer EpaComplianceReportDto/
//                  EpaComplianceFindingDto shapes returned to API callers. Shared by every
//                  EvaluateXComplianceCommandHandler under Compliance.EPA.Compliance so the
//                  translation logic is written once. Additive-only evolution.
// ============================================================================
using DomainEpaComplianceReport = PlantSafety.Domain.Compliance.EPA.Shared.ValueObjects.EpaComplianceReport;
using DomainEpaComplianceFinding = PlantSafety.Domain.Compliance.EPA.Shared.ValueObjects.EpaComplianceFinding;

namespace PlantSafety.Application.Compliance.EPA.Compliance.Models;

/// <summary>Maps Domain-layer EPA compliance-engine output onto Application-layer DTOs.</summary>
public static class EpaComplianceReportMapper
{
    /// <summary>Maps a Domain <see cref="DomainEpaComplianceReport"/> onto an <see cref="EpaComplianceReportDto"/>.</summary>
    public static EpaComplianceReportDto ToDto(DomainEpaComplianceReport report) => new()
    {
        ProgramCode = report.ProgramCode,
        EvaluatedAtUtc = report.EvaluatedAtUtc.Value,
        Violations = report.Violations.Select(ToDto).ToList(),
        Warnings = report.Warnings.Select(ToDto).ToList(),
        RequiredActions = report.RequiredActions.ToList(),
        IsCompliant = report.IsCompliant
    };

    /// <summary>Maps a Domain <see cref="DomainEpaComplianceFinding"/> onto an <see cref="EpaComplianceFindingDto"/>.</summary>
    public static EpaComplianceFindingDto ToDto(DomainEpaComplianceFinding finding) => new()
    {
        Citation = finding.Citation,
        Description = finding.Description,
        Severity = finding.Severity,
        RelatedEntityId = finding.RelatedEntityId
    };
}
