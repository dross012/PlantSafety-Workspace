// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetIncidentPredictionQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Queries;
using PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Repositories;

namespace PlantSafety.Application.Analytics.Predictive.IncidentPrediction.Handlers;

/// <summary>Handles <see cref="GetIncidentPredictionQuery"/>.</summary>
public sealed class GetIncidentPredictionQueryHandler : IRequestHandler<GetIncidentPredictionQuery, Result<IncidentPredictionQueryResult>>
{
    private readonly IIncidentPredictionRepository _repository;
    private readonly ILogger<GetIncidentPredictionQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIncidentPredictionQueryHandler(IIncidentPredictionRepository repository, ILogger<GetIncidentPredictionQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentPredictionQueryResult>> Handle(GetIncidentPredictionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetIncidentPredictionsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<IncidentPredictionQueryResult>.Success(new IncidentPredictionQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve IncidentPrediction for Tenant {TenantId}", request.TenantId);

            return Result<IncidentPredictionQueryResult>.Failure(new ApplicationError(
                "Analytics.Predictive.IncidentPrediction.RetrievalFailed",
                "Unable to retrieve IncidentPrediction data at this time.",
                Guid.NewGuid()));
        }
    }
}
