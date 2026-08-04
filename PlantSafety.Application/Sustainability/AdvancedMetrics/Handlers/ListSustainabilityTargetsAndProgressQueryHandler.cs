// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           ListSustainabilityTargetsAndProgressQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handles ListSustainabilityTargetsAndProgressQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Repositories;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Handlers;

/// <summary>Handles <see cref="ListSustainabilityTargetsAndProgressQuery"/>.</summary>
public sealed class ListSustainabilityTargetsAndProgressQueryHandler : IRequestHandler<ListSustainabilityTargetsAndProgressQuery, Result<IReadOnlyList<SustainabilityTargetProgressDto>>>
{
    private readonly IAdvancedMetricsReadRepository _repository;
    private readonly ILogger<ListSustainabilityTargetsAndProgressQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListSustainabilityTargetsAndProgressQueryHandler(IAdvancedMetricsReadRepository repository, ILogger<ListSustainabilityTargetsAndProgressQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<SustainabilityTargetProgressDto>>> Handle(ListSustainabilityTargetsAndProgressQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListSustainabilityTargetsAndProgressAsync(request.AdvancedMetricsLedgerId, request.TenantId, cancellationToken);
            return Result<IReadOnlyList<SustainabilityTargetProgressDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list sustainability targets for Ledger {AdvancedMetricsLedgerId}", request.AdvancedMetricsLedgerId);

            return Result<IReadOnlyList<SustainabilityTargetProgressDto>>.Failure(new ApplicationError(
                "Sustainability.AdvancedMetrics.SustainabilityTargets.RetrievalFailed",
                "Unable to retrieve sustainability target progress data at this time.",
                Guid.NewGuid()));
        }
    }
}
