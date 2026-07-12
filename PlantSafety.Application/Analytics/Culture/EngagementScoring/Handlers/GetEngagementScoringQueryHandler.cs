// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetEngagementScoringQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Culture.EngagementScoring.Queries;
using PlantSafety.Application.Analytics.Culture.EngagementScoring.Repositories;

namespace PlantSafety.Application.Analytics.Culture.EngagementScoring.Handlers;

/// <summary>Handles <see cref="GetEngagementScoringQuery"/>.</summary>
public sealed class GetEngagementScoringQueryHandler : IRequestHandler<GetEngagementScoringQuery, Result<EngagementScoringQueryResult>>
{
    private readonly IEngagementScoringRepository _repository;
    private readonly ILogger<GetEngagementScoringQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEngagementScoringQueryHandler(IEngagementScoringRepository repository, ILogger<GetEngagementScoringQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EngagementScoringQueryResult>> Handle(GetEngagementScoringQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEngagementScoresAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EngagementScoringQueryResult>.Success(new EngagementScoringQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EngagementScoring for Tenant {TenantId}", request.TenantId);

            return Result<EngagementScoringQueryResult>.Failure(new ApplicationError(
                "Analytics.Culture.EngagementScoring.RetrievalFailed",
                "Unable to retrieve EngagementScoring data at this time.",
                Guid.NewGuid()));
        }
    }
}
