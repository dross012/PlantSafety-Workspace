// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Segregation
//  Layer:          Application
//  File:           EvaluateLoadQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for EvaluateLoadQuery. Wraps SegregationEngine.EvaluateSegregation
//                  directly, mapping Domain-layer value objects to Application-layer DTOs.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects;
using PlantSafety.Domain.Compliance.PHMSA.Classification.ValueObjects.Ids;
using PlantSafety.Domain.Compliance.PHMSA.Segregation.Services;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Placarding.Commands;
using PlantSafety.Application.Compliance.PHMSA.Segregation.Models;
using PlantSafety.Application.Compliance.PHMSA.Segregation.Queries;

namespace PlantSafety.Application.Compliance.PHMSA.Segregation.Handlers;

/// <summary>Handles <see cref="EvaluateLoadQuery"/>.</summary>
public sealed class EvaluateLoadQueryHandler : IRequestHandler<EvaluateLoadQuery, Result<SegregationEvaluationResultDto>>
{
    private readonly SegregationEngine _segregationEngine;
    private readonly ILogger<EvaluateLoadQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EvaluateLoadQueryHandler(SegregationEngine segregationEngine, ILogger<EvaluateLoadQueryHandler> logger)
    {
        _segregationEngine = segregationEngine;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<Result<SegregationEvaluationResultDto>> Handle(EvaluateLoadQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = Evaluate(_segregationEngine, request.LoadItems);
            return Task.FromResult(Result<SegregationEvaluationResultDto>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to evaluate segregation for Tenant {TenantId}", request.TenantId);

            return Task.FromResult(Result<SegregationEvaluationResultDto>.Failure(new ApplicationError(
                "Compliance.PHMSA.Segregation.EvaluationFailed",
                "Unable to evaluate segregation for the proposed load at this time.",
                Guid.NewGuid())));
        }
    }

    /// <summary>
    /// Shared evaluation core reused by <see cref="EvaluateLoadQueryHandler"/> and
    /// <see cref="SuggestLoadPlanQueryHandler"/> so both queries delegate to the same engine call
    /// and DTO mapping.
    /// </summary>
    internal static SegregationEvaluationResultDto Evaluate(SegregationEngine engine, IReadOnlyList<SegregationLoadItemDto> loadItems)
    {
        var items = new List<(HazmatMaterialId MaterialId, HazardClass HazardClass)>();
        foreach (var item in loadItems ?? [])
        {
            if (!HazardClassParser.TryParse(item.HazardClass, out var hazardClass))
                continue;

            items.Add((HazmatMaterialId.FromGuid(item.MaterialId), hazardClass));
        }

        var evaluation = engine.EvaluateSegregation(new SegregationEvaluationInput(items));

        var violations = evaluation.Violations
            .Select(v => new SegregationViolationDto(v.ClassA.ToString(), v.ClassB.ToString(), v.Reason, v.Severity.ToString()))
            .ToList();

        return new SegregationEvaluationResultDto(evaluation.IsCompliant, violations, evaluation.RecommendedLoadPlan);
    }
}
