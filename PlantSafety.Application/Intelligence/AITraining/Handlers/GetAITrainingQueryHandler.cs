// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetAITrainingQuery.
// SchemaVersion: 1
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Intelligence.AITraining.Queries;
using PlantSafety.Application.Intelligence.AITraining.Repositories;

namespace PlantSafety.Application.Intelligence.AITraining.Handlers;

/// <summary>Handles <see cref="GetAITrainingQuery"/>.</summary>
public sealed class GetAITrainingQueryHandler : IRequestHandler<GetAITrainingQuery, Result<AITrainingQueryResult>>
{
    private readonly IAITrainingRepository _repository;
    private readonly ILogger<GetAITrainingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAITrainingQueryHandler(IAITrainingRepository repository, ILogger<GetAITrainingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AITrainingQueryResult>> Handle(GetAITrainingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetTrainingJobsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<AITrainingQueryResult>.Success(new AITrainingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AITraining data for Tenant {TenantId}", request.TenantId);

            return Result<AITrainingQueryResult>.Failure(new ApplicationError(
                "Intelligence.AITraining.RetrievalFailed",
                "Unable to retrieve AITraining data at this time.",
                Guid.NewGuid()));
        }
    }
}