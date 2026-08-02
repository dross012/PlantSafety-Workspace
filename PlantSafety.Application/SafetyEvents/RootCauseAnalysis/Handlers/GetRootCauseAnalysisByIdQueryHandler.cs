// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.RootCauseAnalysis
//  Layer:          Application
//  File:           GetRootCauseAnalysisByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetRootCauseAnalysisByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a
//                  NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors
//                  PlantSafety.Application.Operational.PredictiveMaintenance.Handlers
//                  .GetPredictiveMaintenanceAlertByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Models;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Queries;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Repositories;

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Handlers;

/// <summary>Handles <see cref="GetRootCauseAnalysisByIdQuery"/>.</summary>
public sealed class GetRootCauseAnalysisByIdQueryHandler : IRequestHandler<GetRootCauseAnalysisByIdQuery, Result<RootCauseAnalysisDetailDto>>
{
    private readonly IRootCauseAnalysisRepository _repository;
    private readonly ILogger<GetRootCauseAnalysisByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRootCauseAnalysisByIdQueryHandler(IRootCauseAnalysisRepository repository, ILogger<GetRootCauseAnalysisByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RootCauseAnalysisDetailDto>> Handle(GetRootCauseAnalysisByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.RootCauseAnalysisId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("RootCauseAnalysis {RootCauseAnalysisId} not found for Tenant {TenantId}", request.RootCauseAnalysisId, request.TenantId);

                return Result<RootCauseAnalysisDetailDto>.Failure(new ApplicationError(
                    "SafetyEvents.RootCauseAnalysis.NotFound",
                    "The requested root cause analysis could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<RootCauseAnalysisDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RootCauseAnalysis {RootCauseAnalysisId}", request.RootCauseAnalysisId);

            return Result<RootCauseAnalysisDetailDto>.Failure(new ApplicationError(
                "SafetyEvents.RootCauseAnalysis.RetrievalFailed",
                "Unable to retrieve root cause analysis data at this time.",
                Guid.NewGuid()));
        }
    }
}
