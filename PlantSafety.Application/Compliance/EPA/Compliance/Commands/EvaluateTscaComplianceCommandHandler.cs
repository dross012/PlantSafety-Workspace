// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance.Tsca
//  Layer:          Application
//  File:           EvaluateTscaComplianceCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateTscaComplianceCommand. Maps the command's caller-supplied
//                  chemical records directly into TscaComplianceInput (no repository involved --
//                  see command header for why) and invokes
//                  TscaChemicalComplianceEngine.EvaluateCompliance(...), translating the
//                  Domain-layer Result&lt;EpaComplianceReport&gt; into the Application-layer
//                  Result&lt;EpaComplianceReportDto&gt;. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.EPA.Tsca.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Compliance.Models;

namespace PlantSafety.Application.Compliance.EPA.Compliance.Commands;

/// <summary>Handles <see cref="EvaluateTscaComplianceCommand"/>.</summary>
public sealed class EvaluateTscaComplianceCommandHandler : IRequestHandler<EvaluateTscaComplianceCommand, Result<EpaComplianceReportDto>>
{
    private static readonly TscaChemicalComplianceEngine Engine = new();

    private readonly ILogger<EvaluateTscaComplianceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateTscaComplianceCommandHandler(ILogger<EvaluateTscaComplianceCommandHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<EpaComplianceReportDto>> Handle(EvaluateTscaComplianceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var chemicals = request.Chemicals.Select(c => new TscaChemicalRecord(
                ChemicalName: c.ChemicalName,
                CasNumber: c.CasNumber,
                IsOnTscaInventory: c.IsOnTscaInventory,
                IsNewChemicalNotYetManufactured: c.IsNewChemicalNotYetManufactured,
                PlannedManufactureOrImportDateUtc: c.PlannedManufactureOrImportDateUtc.HasValue ? UtcTimestamp.From(c.PlannedManufactureOrImportDateUtc.Value) : null,
                IsSubjectToSnur: c.IsSubjectToSnur,
                HasSubmittedRequiredNotice: c.HasSubmittedRequiredNotice,
                NoticeSubmittedAtUtc: c.NoticeSubmittedAtUtc.HasValue ? UtcTimestamp.From(c.NoticeSubmittedAtUtc.Value) : null,
                IsRestrictedUnderSection5f: c.IsRestrictedUnderSection5f)).ToList();

            var input = new TscaComplianceInput(
                Chemicals: chemicals,
                EvaluatedAtUtc: UtcTimestamp.Now());

            var evaluation = Engine.EvaluateCompliance(input);

            if (!evaluation.IsSuccess)
            {
                return Task.FromResult(Result<EpaComplianceReportDto>.Failure(new ApplicationError(
                    "Compliance.EPA.TscaEvaluationFailed",
                    evaluation.Error ?? "Unable to evaluate TSCA compliance for the supplied chemical inventory.",
                    Guid.NewGuid())));
            }

            return Task.FromResult(Result<EpaComplianceReportDto>.Success(EpaComplianceReportMapper.ToDto(evaluation.Value!)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate TSCA compliance for Tenant {TenantId}", request.TenantId);

            return Task.FromResult(Result<EpaComplianceReportDto>.Failure(new ApplicationError(
                "Compliance.EPA.TscaEvaluationFailed",
                "Unable to evaluate TSCA compliance at this time.",
                Guid.NewGuid())));
        }
    }
}
