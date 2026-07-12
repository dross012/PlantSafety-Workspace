// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetPredictiveAnalyticsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Queries;
using PlantSafety.Application.Analytics.PredictiveAnalytics.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.PredictiveAnalytics.Handlers;

/// <summary>Handles <see cref="GetPredictiveAnalyticsQuery"/>.</summary>
public sealed class GetPredictiveAnalyticsQueryHandler : IRequestHandler<GetPredictiveAnalyticsQuery, Result<PredictiveAnalyticsQueryResult>>
{
    private readonly IPredictiveAnalyticsRepository _repository;
    private readonly ILogger<GetPredictiveAnalyticsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveAnalyticsQueryHandler(IPredictiveAnalyticsRepository repository, ILogger<GetPredictiveAnalyticsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveAnalyticsQueryResult>> Handle(GetPredictiveAnalyticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPredictionsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PredictiveAnalyticsQueryResult>.Success(new PredictiveAnalyticsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PredictiveAnalytics for Tenant {TenantId}", request.TenantId);

            return Result<PredictiveAnalyticsQueryResult>.Failure(new ApplicationError(
                "Analytics.PredictiveAnalytics.RetrievalFailed",
                "Unable to retrieve predictive analytics at this time.",
                Guid.NewGuid()));
        }
    }
}
