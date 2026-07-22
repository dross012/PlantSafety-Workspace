// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetCulturalPrecursorMetricsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scorecards.CulturalPrecursorMetrics.Queries;
using PlantSafety.Application.Scorecards.CulturalPrecursorMetrics.Repositories;

namespace PlantSafety.Application.Scorecards.CulturalPrecursorMetrics.Handlers;

/// <summary>Handles <see cref="GetCulturalPrecursorMetricsQuery"/>.</summary>
public sealed class GetCulturalPrecursorMetricsQueryHandler : IRequestHandler<GetCulturalPrecursorMetricsQuery, Result<CulturalPrecursorMetricsQueryResult>>
{
    private readonly ICulturalPrecursorMetricsRepository _repository;
    private readonly ILogger<GetCulturalPrecursorMetricsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCulturalPrecursorMetricsQueryHandler(ICulturalPrecursorMetricsRepository repository, ILogger<GetCulturalPrecursorMetricsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CulturalPrecursorMetricsQueryResult>> Handle(GetCulturalPrecursorMetricsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCulturalPrecursorMetricsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CulturalPrecursorMetricsQueryResult>.Success(new CulturalPrecursorMetricsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CulturalPrecursorMetrics for Tenant {TenantId}", request.TenantId);

            return Result<CulturalPrecursorMetricsQueryResult>.Failure(new ApplicationError(
                "Scorecards.CulturalPrecursorMetrics.RetrievalFailed",
                "Unable to retrieve CulturalPrecursorMetrics data at this time.",
                Guid.NewGuid()));
        }
    }
}