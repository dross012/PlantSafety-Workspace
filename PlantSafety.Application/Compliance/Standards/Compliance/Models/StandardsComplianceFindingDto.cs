// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance
//  Layer:          Application
//  File:           StandardsComplianceFindingDto.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Application-layer projection of the Domain-layer
//                  PlantSafety.Domain.Compliance.Standards.Shared.ValueObjects
//                  .StandardsComplianceFinding record, returned to API callers by every
//                  EvaluateXComplianceCommand/EvaluateXStandardsCommand handler under
//                  Compliance.Standards.Compliance (CraneAndHoist, ControlSystemSafety,
//                  MachineSafety, RiskAssessment, OperationalSafety, PressureVessel). Mirrors
//                  Compliance.EPA.Compliance.Models.EpaComplianceFindingDto exactly (same shape,
//                  same "no bleed across module boundaries" convention -- a sibling, independently
//                  declared type rather than a reused/shared one). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Compliance.Standards.Compliance.Models;

/// <summary>A single compliance finding (violation, warning, or advisory) within a <see cref="StandardsComplianceReportDto"/>.</summary>
public sealed record StandardsComplianceFindingDto
{
    /// <summary>Regulatory/standards citation supporting this finding (e.g. "ASME B30 periodic inspection interval").</summary>
    public required string Citation { get; init; }

    /// <summary>Human-readable description of the finding.</summary>
    public required string Description { get; init; }

    /// <summary>Severity classification (e.g. Critical, High, Medium, Advisory).</summary>
    public required string Severity { get; init; }

    /// <summary>Identifier of the related entity (asset, component, guard, permit, etc.), if applicable.</summary>
    public string? RelatedEntityId { get; init; }
}
