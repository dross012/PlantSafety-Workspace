// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance
//  Layer:          Application
//  File:           EpaComplianceFindingDto.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Application-layer projection of the Domain-layer
//                  PlantSafety.Domain.Compliance.EPA.Shared.ValueObjects.EpaComplianceFinding
//                  record, returned to API callers by every EvaluateXComplianceCommand handler
//                  under Compliance.EPA.Compliance. Named EpaComplianceFindingDto (not the bare
//                  "EPA..." spelling) to avoid collision with the pre-existing, unrelated
//                  bare-"EPA" identifiers used by Compliance.Certification.EPA and
//                  Compliance.RegulatoryCalendar.Deadlines.EPA. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.EPA.Compliance.Models;

/// <summary>A single compliance finding (violation, warning, or advisory) within an <see cref="EpaComplianceReportDto"/>.</summary>
public sealed record EpaComplianceFindingDto
{
    /// <summary>Regulatory citation supporting this finding (e.g. "40 CFR 262.17(b)").</summary>
    public required string Citation { get; init; }

    /// <summary>Human-readable description of the finding.</summary>
    public required string Description { get; init; }

    /// <summary>Severity classification (e.g. Critical, High, Medium, Advisory).</summary>
    public required string Severity { get; init; }

    /// <summary>Identifier of the related entity (container, manifest, permit, etc.), if applicable.</summary>
    public string? RelatedEntityId { get; init; }
}
