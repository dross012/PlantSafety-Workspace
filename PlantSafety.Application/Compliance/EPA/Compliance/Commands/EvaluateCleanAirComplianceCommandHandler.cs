// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance.CleanAirAct
//  Layer:          Application
//  File:           EvaluateCleanAirComplianceCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateCleanAirComplianceCommand. Maps the command's
//                  caller-supplied emission-source records directly into
//                  CleanAirComplianceInput (no repository involved -- see command header for why)
//                  and invokes CleanAirComplianceEngine.EvaluateCompliance(...), translating the
//                  Domain-layer Result&lt;EpaComplianceReport&gt; into the Application-layer
//                  Result&lt;EpaComplianceReportDto&gt;. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.EPA.CleanAirAct.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Compliance.Models;

namespace PlantSafety.Application.Compliance.EPA.Compliance.Commands;

/// <summary>Handles <see cref="EvaluateCleanAirComplianceCommand"/>.</summary>
public sealed class EvaluateCleanAirComplianceCommandHandler : IRequestHandler<EvaluateCleanAirComplianceCommand, Result<EpaComplianceReportDto>>
{
    private static readonly CleanAirComplianceEngine Engine = new();

    private readonly ILogger<EvaluateCleanAirComplianceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateCleanAirComplianceCommandHandler(ILogger<EvaluateCleanAirComplianceCommandHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<EpaComplianceReportDto>> Handle(EvaluateCleanAirComplianceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emissionSources = request.EmissionSources.Select(s => new EmissionSourceRecord(
                SourceName: s.SourceName,
                AnyPollutantTonsPerYear: s.AnyPollutantTonsPerYear,
                SingleHapTonsPerYear: s.SingleHapTonsPerYear,
                CombinedHapTonsPerYear: s.CombinedHapTonsPerYear,
                IsSubjectToNsps: s.IsSubjectToNsps,
                IsSubjectToNeshap: s.IsSubjectToNeshap,
                HasTitleVPermit: s.HasTitleVPermit,
                TitleVPermitExpiresAtUtc: s.TitleVPermitExpiresAtUtc.HasValue ? UtcTimestamp.From(s.TitleVPermitExpiresAtUtc.Value) : null)).ToList();

            var input = new CleanAirComplianceInput(
                EmissionSources: emissionSources,
                EvaluatedAtUtc: UtcTimestamp.Now());

            var evaluation = Engine.EvaluateCompliance(input);

            if (!evaluation.IsSuccess)
            {
                return Task.FromResult(Result<EpaComplianceReportDto>.Failure(new ApplicationError(
                    "Compliance.EPA.CleanAirEvaluationFailed",
                    evaluation.Error ?? "Unable to evaluate Clean Air Act compliance for the supplied emission sources.",
                    Guid.NewGuid())));
            }

            return Task.FromResult(Result<EpaComplianceReportDto>.Success(EpaComplianceReportMapper.ToDto(evaluation.Value!)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate Clean Air Act compliance for Tenant {TenantId}", request.TenantId);

            return Task.FromResult(Result<EpaComplianceReportDto>.Failure(new ApplicationError(
                "Compliance.EPA.CleanAirEvaluationFailed",
                "Unable to evaluate Clean Air Act compliance at this time.",
                Guid.NewGuid())));
        }
    }
}
