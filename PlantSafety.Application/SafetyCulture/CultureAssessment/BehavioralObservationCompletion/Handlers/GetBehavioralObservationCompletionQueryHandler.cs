// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetBehavioralObservationCompletionQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Repositories;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.BehavioralObservationCompletion.Handlers;

/// <summary>Handles <see cref="GetBehavioralObservationCompletionQuery"/>.</summary>
public sealed class GetBehavioralObservationCompletionQueryHandler : IRequestHandler<GetBehavioralObservationCompletionQuery, Result<BehavioralObservationCompletionQueryResult>>
{
    private readonly IBehavioralObservationCompletionRepository _repository;
    private readonly ILogger<GetBehavioralObservationCompletionQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetBehavioralObservationCompletionQueryHandler(IBehavioralObservationCompletionRepository repository, ILogger<GetBehavioralObservationCompletionQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<BehavioralObservationCompletionQueryResult>> Handle(GetBehavioralObservationCompletionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<BehavioralObservationCompletionQueryResult>.Success(new BehavioralObservationCompletionQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve BehavioralObservationCompletion for Tenant {TenantId}", request.TenantId);

            return Result<BehavioralObservationCompletionQueryResult>.Failure(new ApplicationError(
                "SafetyCulture.CultureAssessment.BehavioralObservationCompletion.RetrievalFailed",
                "Unable to retrieve Behavioral Observation Completion data at this time.",
                Guid.NewGuid()));
        }
    }
}