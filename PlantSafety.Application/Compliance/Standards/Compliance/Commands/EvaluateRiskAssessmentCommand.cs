// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.RiskAssessment
//  Layer:          Application
//  File:           EvaluateRiskAssessmentCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to evaluate whether a documented machinery risk assessment meets the
//                  minimum structural requirements of ISO 12100 / ANSI B11.0-2023. This codebase
//                  has no persisted machinery-risk-assessment aggregate (confirmed absent -- see
//                  the Domain engine's own RiskAssessmentInput.cs remarks), so the assessment's
//                  own recorded content is carried directly on this command rather than loaded
//                  from a nonexistent repository (same pattern as
//                  Compliance.EPA.Compliance.Commands.EvaluateTscaComplianceCommand). Carries the
//                  full command envelope (CorrelationId/CausationId/CommandId/RequestedAtUtc/
//                  ApiVersionToken) per Handbook Rule 2.1. Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;
using PlantSafety.Domain.Compliance.Standards.RiskAssessment.Enums;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>A single documented risk-reduction measure and the ISO 12100 hierarchy step it represents, mirroring the Domain engine's RiskReductionMeasureRecord input shape.</summary>
/// <param name="Step">Which of the three ISO 12100 hierarchy steps this measure belongs to.</param>
/// <param name="Description">Free-text description of the measure as recorded in the assessment.</param>
public sealed record RiskReductionMeasureInput(
    RiskReductionHierarchyStep Step,
    string Description);

/// <summary>Evaluates whether a documented machinery risk assessment meets the minimum structural requirements of ISO 12100 / ANSI B11.0.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="ActorId">Identity of the user requesting the evaluation.</param>
/// <param name="EquipmentIdentifier">Identifier of the machine/equipment the assessment covers.</param>
/// <param name="Methodology">The named methodology used, e.g. "ISO 12100", "ANSI B11.0 Annex C", or "NSC".</param>
/// <param name="IdentifiedHazards">The hazards systematically identified during the assessment.</param>
/// <param name="SeverityEstimated">Whether the assessment recorded a severity estimate for identified hazards.</param>
/// <param name="ExposureEstimated">Whether the assessment recorded an exposure (frequency/duration) estimate.</param>
/// <param name="ProbabilityEstimated">Whether the assessment recorded a probability-of-occurrence estimate.</param>
/// <param name="RiskReductionMeasures">The risk-reduction measures documented, each tagged with its hierarchy step.</param>
/// <param name="IsIntegratedSystem">Whether this equipment is part of a multi-machine integrated system requiring its own system-level risk assessment.</param>
/// <param name="AssessmentCompletedAtUtc">When the documented risk assessment was completed.</param>
public sealed record EvaluateRiskAssessmentCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid ActorId,
    string EquipmentIdentifier,
    string Methodology,
    IReadOnlyList<string> IdentifiedHazards,
    bool SeverityEstimated,
    bool ExposureEstimated,
    bool ProbabilityEstimated,
    IReadOnlyList<RiskReductionMeasureInput> RiskReductionMeasures,
    bool IsIntegratedSystem,
    DateTime AssessmentCompletedAtUtc) : ICommand<Result<StandardsComplianceReportDto>>;
