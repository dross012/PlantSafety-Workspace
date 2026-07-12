// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetBehaviorQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Culture.Behavior.Queries;
using PlantSafety.Application.Analytics.Culture.Behavior.Repositories;

namespace PlantSafety.Application.Analytics.Culture.Behavior.Handlers;

/// <summary>Handles <see cref="GetBehaviorQuery"/>.</summary>
public sealed class GetBehaviorQueryHandler : IRequestHandler<GetBehaviorQuery, Result<BehaviorQueryResult>>
{
    private readonly IBehaviorRepository _repository;
    private readonly ILogger<GetBehaviorQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetBehaviorQueryHandler(IBehaviorRepository repository, ILogger<GetBehaviorQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<BehaviorQueryResult>> Handle(GetBehaviorQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetBehaviorObservationsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<BehaviorQueryResult>.Success(new BehaviorQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Behavior for Tenant {TenantId}", request.TenantId);

            return Result<BehaviorQueryResult>.Failure(new ApplicationError(
                "Analytics.Culture.Behavior.RetrievalFailed",
                "Unable to retrieve Behavior data at this time.",
                Guid.NewGuid()));
        }
    }
}
