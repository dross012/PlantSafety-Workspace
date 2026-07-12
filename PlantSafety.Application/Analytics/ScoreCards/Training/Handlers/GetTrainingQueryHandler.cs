// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetTrainingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.ScoreCards.Training.Queries;
using PlantSafety.Application.Analytics.ScoreCards.Training.Repositories;

namespace PlantSafety.Application.Analytics.ScoreCards.Training.Handlers;

/// <summary>Handles <see cref="GetTrainingQuery"/>.</summary>
public sealed class GetTrainingQueryHandler : IRequestHandler<GetTrainingQuery, Result<TrainingQueryResult>>
{
    private readonly ITrainingRepository _repository;
    private readonly ILogger<GetTrainingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetTrainingQueryHandler(ITrainingRepository repository, ILogger<GetTrainingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TrainingQueryResult>> Handle(GetTrainingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetTrainingScoresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<TrainingQueryResult>.Success(new TrainingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Training for Tenant {TenantId}", request.TenantId);

            return Result<TrainingQueryResult>.Failure(new ApplicationError(
                "Analytics.ScoreCards.Training.RetrievalFailed",
                "Unable to retrieve Training data at this time.",
                Guid.NewGuid()));
        }
    }
}
