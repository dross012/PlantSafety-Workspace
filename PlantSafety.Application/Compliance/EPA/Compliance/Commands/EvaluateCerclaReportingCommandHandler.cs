// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance.CerclaSara
//  Layer:          Application
//  File:           EvaluateCerclaReportingCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateCerclaReportingCommand. Maps the command's caller-supplied
//                  release details directly into CerclaReleaseInput (no repository involved --
//                  see command header for why) and invokes
//                  CerclaReleaseReportingEngine.EvaluateReleaseReporting(...), translating the
//                  Domain-layer Result&lt;EpaComplianceReport&gt; into the Application-layer
//                  Result&lt;EpaComplianceReportDto&gt;. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.EPA.CerclaSara.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Compliance.Models;

namespace PlantSafety.Application.Compliance.EPA.Compliance.Commands;

/// <summary>Handles <see cref="EvaluateCerclaReportingCommand"/>.</summary>
public sealed class EvaluateCerclaReportingCommandHandler : IRequestHandler<EvaluateCerclaReportingCommand, Result<EpaComplianceReportDto>>
{
    private static readonly CerclaReleaseReportingEngine Engine = new();

    private readonly ILogger<EvaluateCerclaReportingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateCerclaReportingCommandHandler(ILogger<EvaluateCerclaReportingCommandHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<EpaComplianceReportDto>> Handle(EvaluateCerclaReportingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var input = new CerclaReleaseInput(
                LinkedIncidentManagementId: request.LinkedIncidentManagementId,
                SubstanceName: request.SubstanceName,
                CasNumber: request.CasNumber,
                ReleasedQuantity: request.ReleasedQuantity,
                QuantityUnit: request.QuantityUnit,
                ReportableQuantityThreshold: request.ReportableQuantityThreshold,
                IsExtremelyHazardousSubstance: request.IsExtremelyHazardousSubstance,
                ReleaseOccurredAtUtc: UtcTimestamp.From(request.ReleaseOccurredAtUtc),
                EvaluatedAtUtc: UtcTimestamp.Now());

            var evaluation = Engine.EvaluateReleaseReporting(input);

            if (!evaluation.IsSuccess)
            {
                return Task.FromResult(Result<EpaComplianceReportDto>.Failure(new ApplicationError(
                    "Compliance.EPA.CerclaEvaluationFailed",
                    evaluation.Error ?? "Unable to evaluate CERCLA/EPCRA release-reporting obligations for the supplied release.",
                    Guid.NewGuid())));
            }

            return Task.FromResult(Result<EpaComplianceReportDto>.Success(EpaComplianceReportMapper.ToDto(evaluation.Value!)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate CERCLA/EPCRA release reporting for Tenant {TenantId}", request.TenantId);

            return Task.FromResult(Result<EpaComplianceReportDto>.Failure(new ApplicationError(
                "Compliance.EPA.CerclaEvaluationFailed",
                "Unable to evaluate CERCLA/EPCRA release-reporting obligations at this time.",
                Guid.NewGuid())));
        }
    }
}
