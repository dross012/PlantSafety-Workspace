// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.EPA.Compliance.CleanWaterAct
//  Layer:          Application
//  File:           EvaluateCleanWaterComplianceCommandHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateCleanWaterComplianceCommand. Loads real DischargePermit
//                  data (per the caller-supplied DischargePermitIds -- see command header for why)
//                  via IDischargePermitRepository, and real active LimitExceedance data for the
//                  site via ILimitExceedanceRepository, projects both into the Domain engine's
//                  DischargePermitSnapshot/LimitExceedanceSnapshot records (computing each
//                  exceedance's RepeatExceedanceCount from the real per-discharge-point occurrence
//                  count within the loaded set), invokes
//                  CleanWaterComplianceEngine.EvaluateCompliance(...), and translates the
//                  Domain-layer Result&lt;EpaComplianceReport&gt; into the Application-layer
//                  Result&lt;EpaComplianceReportDto&gt;. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Compliance.EPA.CleanWaterAct.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.EPA.Compliance.Models;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePermit.Repositories;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Repositories;

namespace PlantSafety.Application.Compliance.EPA.Compliance.Commands;

/// <summary>Handles <see cref="EvaluateCleanWaterComplianceCommand"/>.</summary>
public sealed class EvaluateCleanWaterComplianceCommandHandler : IRequestHandler<EvaluateCleanWaterComplianceCommand, Result<EpaComplianceReportDto>>
{
    private static readonly CleanWaterComplianceEngine Engine = new();

    private readonly IDischargePermitRepository _dischargePermitRepository;
    private readonly ILimitExceedanceRepository _limitExceedanceRepository;
    private readonly ILogger<EvaluateCleanWaterComplianceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateCleanWaterComplianceCommandHandler(
        IDischargePermitRepository dischargePermitRepository,
        ILimitExceedanceRepository limitExceedanceRepository,
        ILogger<EvaluateCleanWaterComplianceCommandHandler> logger)
    {
        _dischargePermitRepository = dischargePermitRepository;
        _limitExceedanceRepository = limitExceedanceRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EpaComplianceReportDto>> Handle(EvaluateCleanWaterComplianceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var permits = new List<DischargePermitSnapshot>();
            foreach (var dischargePermitId in request.DischargePermitIds)
            {
                var permit = await _dischargePermitRepository.GetDischargePermitByIdAsync(request.TenantId, dischargePermitId, cancellationToken);
                if (permit is null)
                    continue;

                permits.Add(new DischargePermitSnapshot(
                    DischargePermitId: permit.DischargePermitId,
                    PermitNumber: permit.PermitNumber,
                    RegulatoryProgram: permit.RegulatoryProgram,
                    EffectiveAtUtc: UtcTimestamp.From(permit.EffectiveAtUtc),
                    ExpiresAtUtc: UtcTimestamp.From(permit.ExpiresAtUtc),
                    Status: permit.Status));
            }

            var activeExceedances = await _limitExceedanceRepository.ListActiveExceedancesAsync(request.TenantId, request.SiteId, cancellationToken);

            var repeatCountsByDischargePoint = activeExceedances
                .GroupBy(e => e.DischargePointId)
                .ToDictionary(g => g.Key, g => g.Count());

            var exceedances = activeExceedances.Select(e => new LimitExceedanceSnapshot(
                LimitExceedanceId: e.LimitExceedanceId,
                DischargePointId: e.DischargePointId,
                MeasuredValue: e.MeasuredValue,
                LimitValue: e.LimitValue,
                ExceedancePercentage: e.ExceedancePercentage,
                Severity: e.Severity,
                RepeatExceedanceCount: repeatCountsByDischargePoint[e.DischargePointId])).ToList();

            var input = new CleanWaterComplianceInput(
                Permits: permits,
                Exceedances: exceedances,
                FacilityStoresOilAboveSpccThreshold: request.FacilityStoresOilAboveSpccThreshold,
                HasSpccPlan: request.HasSpccPlan,
                SpccPlanLastReviewedAtUtc: request.SpccPlanLastReviewedAtUtc.HasValue ? UtcTimestamp.From(request.SpccPlanLastReviewedAtUtc.Value) : null,
                EvaluatedAtUtc: UtcTimestamp.Now());

            var evaluation = Engine.EvaluateCompliance(input);

            if (!evaluation.IsSuccess)
            {
                return Result<EpaComplianceReportDto>.Failure(new ApplicationError(
                    "Compliance.EPA.CleanWaterEvaluationFailed",
                    evaluation.Error ?? "Unable to evaluate Clean Water Act compliance for the requested site.",
                    Guid.NewGuid()));
            }

            return Result<EpaComplianceReportDto>.Success(EpaComplianceReportMapper.ToDto(evaluation.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate Clean Water Act compliance for Site {SiteId}", request.SiteId);

            return Result<EpaComplianceReportDto>.Failure(new ApplicationError(
                "Compliance.EPA.CleanWaterEvaluationFailed",
                "Unable to evaluate Clean Water Act compliance at this time.",
                Guid.NewGuid()));
        }
    }
}
