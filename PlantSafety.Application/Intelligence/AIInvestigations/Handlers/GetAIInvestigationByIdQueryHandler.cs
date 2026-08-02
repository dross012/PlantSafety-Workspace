// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Intelligence.AIInvestigations
//  Layer:          Application
//  File:           GetAIInvestigationByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetAIInvestigationByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a
//                  NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Handlers
//                  .GetRootCauseAnalysisByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.AIInvestigations.Models;
using PlantSafety.Application.Intelligence.AIInvestigations.Queries;
using PlantSafety.Application.Intelligence.AIInvestigations.Repositories;

namespace PlantSafety.Application.Intelligence.AIInvestigations.Handlers;

/// <summary>Handles <see cref="GetAIInvestigationByIdQuery"/>.</summary>
public sealed class GetAIInvestigationByIdQueryHandler : IRequestHandler<GetAIInvestigationByIdQuery, Result<AIInvestigationDetailDto>>
{
    private readonly IAIInvestigationReadRepository _repository;
    private readonly ILogger<GetAIInvestigationByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAIInvestigationByIdQueryHandler(IAIInvestigationReadRepository repository, ILogger<GetAIInvestigationByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AIInvestigationDetailDto>> Handle(GetAIInvestigationByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.AIInvestigationId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("AIInvestigation {AIInvestigationId} not found for Tenant {TenantId}", request.AIInvestigationId, request.TenantId);

                return Result<AIInvestigationDetailDto>.Failure(new ApplicationError(
                    "Intelligence.AIInvestigation.NotFound",
                    "The requested AI investigation could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<AIInvestigationDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AIInvestigation {AIInvestigationId}", request.AIInvestigationId);

            return Result<AIInvestigationDetailDto>.Failure(new ApplicationError(
                "Intelligence.AIInvestigation.RetrievalFailed",
                "Unable to retrieve AI investigation data at this time.",
                Guid.NewGuid()));
        }
    }
}
