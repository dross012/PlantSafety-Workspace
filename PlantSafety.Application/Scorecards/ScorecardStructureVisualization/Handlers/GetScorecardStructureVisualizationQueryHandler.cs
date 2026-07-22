// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetScorecardStructureVisualizationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scorecards.ScorecardStructureVisualization.Queries;
using PlantSafety.Application.Scorecards.ScorecardStructureVisualization.Repositories;

namespace PlantSafety.Application.Scorecards.ScorecardStructureVisualization.Handlers;

/// <summary>Handles <see cref="GetScorecardStructureVisualizationQuery"/>.</summary>
public sealed class GetScorecardStructureVisualizationQueryHandler : IRequestHandler<GetScorecardStructureVisualizationQuery, Result<ScorecardStructureVisualizationQueryResult>>
{
    private readonly IScorecardStructureVisualizationRepository _repository;
    private readonly ILogger<GetScorecardStructureVisualizationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetScorecardStructureVisualizationQueryHandler(IScorecardStructureVisualizationRepository repository, ILogger<GetScorecardStructureVisualizationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ScorecardStructureVisualizationQueryResult>> Handle(GetScorecardStructureVisualizationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetScorecardStructureVisualizationAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ScorecardStructureVisualizationQueryResult>.Success(new ScorecardStructureVisualizationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ScorecardStructureVisualization for Tenant {TenantId}", request.TenantId);

            return Result<ScorecardStructureVisualizationQueryResult>.Failure(new ApplicationError(
                "Scorecards.ScorecardStructureVisualization.RetrievalFailed",
                "Unable to retrieve ScorecardStructureVisualization data at this time.",
                Guid.NewGuid()));
        }
    }
}