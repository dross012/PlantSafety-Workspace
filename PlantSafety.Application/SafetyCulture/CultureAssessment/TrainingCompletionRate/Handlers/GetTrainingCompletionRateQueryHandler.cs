// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetTrainingCompletionRateQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Repositories;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.TrainingCompletionRate.Handlers;

/// <summary>Handles <see cref="GetTrainingCompletionRateQuery"/>.</summary>
public sealed class GetTrainingCompletionRateQueryHandler : IRequestHandler<GetTrainingCompletionRateQuery, Result<TrainingCompletionRateQueryResult>>
{
    private readonly ITrainingCompletionRateRepository _repository;
    private readonly ILogger<GetTrainingCompletionRateQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetTrainingCompletionRateQueryHandler(ITrainingCompletionRateRepository repository, ILogger<GetTrainingCompletionRateQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TrainingCompletionRateQueryResult>> Handle(GetTrainingCompletionRateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<TrainingCompletionRateQueryResult>.Success(new TrainingCompletionRateQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve TrainingCompletionRate for Tenant {TenantId}", request.TenantId);

            return Result<TrainingCompletionRateQueryResult>.Failure(new ApplicationError(
                "SafetyCulture.CultureAssessment.TrainingCompletionRate.RetrievalFailed",
                "Unable to retrieve Training Completion Rate data at this time.",
                Guid.NewGuid()));
        }
    }
}