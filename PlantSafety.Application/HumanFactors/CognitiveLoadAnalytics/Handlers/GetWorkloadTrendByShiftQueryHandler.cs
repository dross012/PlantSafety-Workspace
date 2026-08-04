// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           GetWorkloadTrendByShiftQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handles GetWorkloadTrendByShiftQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Handlers;

/// <summary>Handles <see cref="GetWorkloadTrendByShiftQuery"/>.</summary>
public sealed class GetWorkloadTrendByShiftQueryHandler : IRequestHandler<GetWorkloadTrendByShiftQuery, Result<IReadOnlyList<WorkloadTrendByShiftDto>>>
{
    private readonly ICognitiveLoadAnalyticsReadRepository _repository;
    private readonly ILogger<GetWorkloadTrendByShiftQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkloadTrendByShiftQueryHandler(ICognitiveLoadAnalyticsReadRepository repository, ILogger<GetWorkloadTrendByShiftQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<WorkloadTrendByShiftDto>>> Handle(GetWorkloadTrendByShiftQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetWorkloadTrendByShiftAsync(
                request.TenantId, request.SiteId, request.ShiftType, request.PeriodStartUtc, request.PeriodEndUtc, cancellationToken);

            return Result<IReadOnlyList<WorkloadTrendByShiftDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve workload trend by shift for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<WorkloadTrendByShiftDto>>.Failure(new ApplicationError(
                "HumanFactors.CognitiveLoadAnalytics.WorkloadTrendByShift.RetrievalFailed",
                "Unable to retrieve the workload trend at this time.",
                Guid.NewGuid()));
        }
    }
}
