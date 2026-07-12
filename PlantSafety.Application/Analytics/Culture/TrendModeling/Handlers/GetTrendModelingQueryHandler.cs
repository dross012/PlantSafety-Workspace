// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetTrendModelingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Culture.TrendModeling.Queries;
using PlantSafety.Application.Analytics.Culture.TrendModeling.Repositories;

namespace PlantSafety.Application.Analytics.Culture.TrendModeling.Handlers;

/// <summary>Handles <see cref="GetTrendModelingQuery"/>.</summary>
public sealed class GetTrendModelingQueryHandler : IRequestHandler<GetTrendModelingQuery, Result<TrendModelingQueryResult>>
{
    private readonly ITrendModelingRepository _repository;
    private readonly ILogger<GetTrendModelingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetTrendModelingQueryHandler(ITrendModelingRepository repository, ILogger<GetTrendModelingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TrendModelingQueryResult>> Handle(GetTrendModelingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetTrendModelsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<TrendModelingQueryResult>.Success(new TrendModelingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve TrendModeling for Tenant {TenantId}", request.TenantId);

            return Result<TrendModelingQueryResult>.Failure(new ApplicationError(
                "Analytics.Culture.TrendModeling.RetrievalFailed",
                "Unable to retrieve TrendModeling data at this time.",
                Guid.NewGuid()));
        }
    }
}
