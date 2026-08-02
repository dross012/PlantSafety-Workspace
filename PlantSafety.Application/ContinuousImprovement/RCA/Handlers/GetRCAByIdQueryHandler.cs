// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ContinuousImprovement.RCA
//  Layer:          Application
//  File:           GetRCAByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetRCAByIdQuery. Reads from the read-side repository (optimised
//                  stored proc; no domain-aggregate reconstruction). Returns a NotFound failure
//                  both when the row does not exist and when it belongs to a different tenant, so
//                  tenant membership is never revealed to an unauthorized caller. Mirrors
//                  PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Handlers
//                  .GetRootCauseAnalysisByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ContinuousImprovement.RCA.Models;
using PlantSafety.Application.ContinuousImprovement.RCA.Queries;
using PlantSafety.Application.ContinuousImprovement.RCA.Repositories;

namespace PlantSafety.Application.ContinuousImprovement.RCA.Handlers;

/// <summary>Handles <see cref="GetRCAByIdQuery"/>.</summary>
public sealed class GetRCAByIdQueryHandler : IRequestHandler<GetRCAByIdQuery, Result<RCADetailDto>>
{
    private readonly IRCADetailRepository _repository;
    private readonly ILogger<GetRCAByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRCAByIdQueryHandler(IRCADetailRepository repository, ILogger<GetRCAByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RCADetailDto>> Handle(GetRCAByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.RcaId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("RCA {RcaId} not found for Tenant {TenantId}", request.RcaId, request.TenantId);

                return Result<RCADetailDto>.Failure(new ApplicationError(
                    "ContinuousImprovement.RCA.NotFound",
                    "The requested root cause analysis could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<RCADetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RCA {RcaId}", request.RcaId);

            return Result<RCADetailDto>.Failure(new ApplicationError(
                "ContinuousImprovement.RCA.RetrievalFailed",
                "Unable to retrieve root cause analysis data at this time.",
                Guid.NewGuid()));
        }
    }
}
