// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Standards.Compliance.RiskAssessment
//  Layer:          Application
//  File:           EvaluateRiskAssessmentCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateRiskAssessmentCommand. Maps the command's caller-supplied
//                  risk-assessment content directly into RiskAssessmentInput (no repository
//                  involved -- see command header for why) and invokes
//                  RiskAssessmentEngine.EvaluateRiskAssessment(...), translating the Domain-layer
//                  Result&lt;StandardsComplianceReport&gt; into the Application-layer
//                  Result&lt;StandardsComplianceReportDto&gt;. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.Standards.RiskAssessment.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Standards.Compliance.Models;

namespace PlantSafety.Application.Compliance.Standards.Compliance.Commands;

/// <summary>Handles <see cref="EvaluateRiskAssessmentCommand"/>.</summary>
public sealed class EvaluateRiskAssessmentCommandHandler : IRequestHandler<EvaluateRiskAssessmentCommand, Result<StandardsComplianceReportDto>>
{
    private static readonly RiskAssessmentEngine Engine = new();

    private readonly ILogger<EvaluateRiskAssessmentCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateRiskAssessmentCommandHandler(ILogger<EvaluateRiskAssessmentCommandHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<StandardsComplianceReportDto>> Handle(EvaluateRiskAssessmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var measures = request.RiskReductionMeasures
                .Select(m => new RiskReductionMeasureRecord(m.Step, m.Description))
                .ToList();

            var input = new RiskAssessmentInput(
                EquipmentIdentifier: request.EquipmentIdentifier,
                Methodology: request.Methodology,
                IdentifiedHazards: request.IdentifiedHazards,
                SeverityEstimated: request.SeverityEstimated,
                ExposureEstimated: request.ExposureEstimated,
                ProbabilityEstimated: request.ProbabilityEstimated,
                RiskReductionMeasures: measures,
                IsIntegratedSystem: request.IsIntegratedSystem,
                AssessmentCompletedAtUtc: UtcTimestamp.From(request.AssessmentCompletedAtUtc),
                EvaluatedAtUtc: UtcTimestamp.Now());

            var evaluation = Engine.EvaluateRiskAssessment(input);

            if (!evaluation.IsSuccess)
            {
                return Task.FromResult(Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                    "Compliance.Standards.RiskAssessmentEvaluationFailed",
                    evaluation.Error ?? "Unable to evaluate the supplied risk assessment.",
                    Guid.NewGuid())));
            }

            return Task.FromResult(Result<StandardsComplianceReportDto>.Success(StandardsComplianceReportMapper.ToDto(evaluation.Value!)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate ISO 12100 / ANSI B11.0 risk assessment for Tenant {TenantId}, Equipment {EquipmentIdentifier}", request.TenantId, request.EquipmentIdentifier);

            return Task.FromResult(Result<StandardsComplianceReportDto>.Failure(new ApplicationError(
                "Compliance.Standards.RiskAssessmentEvaluationFailed",
                "Unable to evaluate the supplied risk assessment at this time.",
                Guid.NewGuid())));
        }
    }
}
