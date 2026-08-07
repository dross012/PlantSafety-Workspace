// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Segregation
//  Layer:          Application
//  File:           SuggestLoadPlanQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for SuggestLoadPlanQuery. Reuses
//                  EvaluateLoadQueryHandler.Evaluate (the same SegregationEngine.
//                  EvaluateSegregation call used by EvaluateLoadQueryHandler) and projects only the
//                  RecommendedLoadPlan portion of the result.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.Compliance.PHMSA.Segregation.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Segregation.Queries;

namespace PlantSafety.Application.Compliance.PHMSA.Segregation.Handlers;

/// <summary>Handles <see cref="SuggestLoadPlanQuery"/>.</summary>
public sealed class SuggestLoadPlanQueryHandler : IRequestHandler<SuggestLoadPlanQuery, Result<SuggestLoadPlanResult>>
{
    private readonly SegregationEngine _segregationEngine;
    private readonly ILogger<SuggestLoadPlanQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SuggestLoadPlanQueryHandler(SegregationEngine segregationEngine, ILogger<SuggestLoadPlanQueryHandler> logger)
    {
        _segregationEngine = segregationEngine;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<SuggestLoadPlanResult>> Handle(SuggestLoadPlanQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var evaluation = EvaluateLoadQueryHandler.Evaluate(_segregationEngine, request.LoadItems);

            return Task.FromResult(Result<SuggestLoadPlanResult>.Success(new SuggestLoadPlanResult(
                evaluation.IsCompliant,
                evaluation.RecommendedLoadPlan)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to suggest load plan for Tenant {TenantId}", request.TenantId);

            return Task.FromResult(Result<SuggestLoadPlanResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Segregation.SuggestionFailed",
                "Unable to suggest a load plan for the proposed load at this time.",
                Guid.NewGuid())));
        }
    }
}
