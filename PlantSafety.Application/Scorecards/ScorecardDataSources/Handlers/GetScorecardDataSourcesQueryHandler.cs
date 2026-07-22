// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetScorecardDataSourcesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scorecards.ScorecardDataSources.Queries;
using PlantSafety.Application.Scorecards.ScorecardDataSources.Repositories;

namespace PlantSafety.Application.Scorecards.ScorecardDataSources.Handlers;

/// <summary>Handles <see cref="GetScorecardDataSourcesQuery"/>.</summary>
public sealed class GetScorecardDataSourcesQueryHandler : IRequestHandler<GetScorecardDataSourcesQuery, Result<ScorecardDataSourcesQueryResult>>
{
    private readonly IScorecardDataSourcesRepository _repository;
    private readonly ILogger<GetScorecardDataSourcesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetScorecardDataSourcesQueryHandler(IScorecardDataSourcesRepository repository, ILogger<GetScorecardDataSourcesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScorecardDataSourcesQueryResult>> Handle(GetScorecardDataSourcesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetScorecardDataSourcesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ScorecardDataSourcesQueryResult>.Success(new ScorecardDataSourcesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ScorecardDataSources for Tenant {TenantId}", request.TenantId);

            return Result<ScorecardDataSourcesQueryResult>.Failure(new ApplicationError(
                "Scorecards.ScorecardDataSources.RetrievalFailed",
                "Unable to retrieve ScorecardDataSources data at this time.",
                Guid.NewGuid()));
        }
    }
}