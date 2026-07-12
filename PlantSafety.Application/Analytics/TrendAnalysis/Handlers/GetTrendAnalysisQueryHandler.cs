// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetTrendAnalysisQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.TrendAnalysis.Queries;
using PlantSafety.Application.Analytics.TrendAnalysis.Repositories;

namespace PlantSafety.Application.Analytics.TrendAnalysis.Handlers;

/// <summary>Handles <see cref="GetTrendAnalysisQuery"/>.</summary>
public sealed class GetTrendAnalysisQueryHandler : IRequestHandler<GetTrendAnalysisQuery, Result<TrendAnalysisQueryResult>>
{
    private readonly ITrendAnalysisRepository _repository;
    private readonly ILogger<GetTrendAnalysisQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetTrendAnalysisQueryHandler(ITrendAnalysisRepository repository, ILogger<GetTrendAnalysisQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TrendAnalysisQueryResult>> Handle(GetTrendAnalysisQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetTrendPointsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<TrendAnalysisQueryResult>.Success(new TrendAnalysisQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve TrendAnalysis for Tenant {TenantId}", request.TenantId);

            return Result<TrendAnalysisQueryResult>.Failure(new ApplicationError(
                "Analytics.TrendAnalysis.RetrievalFailed",
                "Unable to retrieve TrendAnalysis data at this time.",
                Guid.NewGuid()));
        }
    }
}
