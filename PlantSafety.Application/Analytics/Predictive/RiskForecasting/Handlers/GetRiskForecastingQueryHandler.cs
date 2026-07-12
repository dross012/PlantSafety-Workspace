// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetRiskForecastingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Predictive.RiskForecasting.Queries;
using PlantSafety.Application.Analytics.Predictive.RiskForecasting.Repositories;

namespace PlantSafety.Application.Analytics.Predictive.RiskForecasting.Handlers;

/// <summary>Handles <see cref="GetRiskForecastingQuery"/>.</summary>
public sealed class GetRiskForecastingQueryHandler : IRequestHandler<GetRiskForecastingQuery, Result<RiskForecastingQueryResult>>
{
    private readonly IRiskForecastingRepository _repository;
    private readonly ILogger<GetRiskForecastingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRiskForecastingQueryHandler(IRiskForecastingRepository repository, ILogger<GetRiskForecastingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RiskForecastingQueryResult>> Handle(GetRiskForecastingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRiskForecastsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RiskForecastingQueryResult>.Success(new RiskForecastingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RiskForecasting for Tenant {TenantId}", request.TenantId);

            return Result<RiskForecastingQueryResult>.Failure(new ApplicationError(
                "Analytics.Predictive.RiskForecasting.RetrievalFailed",
                "Unable to retrieve RiskForecasting data at this time.",
                Guid.NewGuid()));
        }
    }
}
