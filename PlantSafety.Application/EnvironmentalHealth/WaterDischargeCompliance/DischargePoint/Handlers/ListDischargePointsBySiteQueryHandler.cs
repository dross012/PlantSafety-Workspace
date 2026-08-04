// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListDischargePointsBySiteQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Repositories;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.Handlers;

/// <summary>Handles <see cref="ListDischargePointsBySiteQuery"/>.</summary>
public sealed class ListDischargePointsBySiteQueryHandler : IRequestHandler<ListDischargePointsBySiteQuery, Result<ListDischargePointsBySiteQueryResult>>
{
    private readonly IDischargePointRepository _repository;
    private readonly ILogger<ListDischargePointsBySiteQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListDischargePointsBySiteQueryHandler(IDischargePointRepository repository, ILogger<ListDischargePointsBySiteQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListDischargePointsBySiteQueryResult>> Handle(ListDischargePointsBySiteQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListDischargePointsBySiteAsync(request.TenantId, request.SiteId, cancellationToken);

            return Result<ListDischargePointsBySiteQueryResult>.Success(new ListDischargePointsBySiteQueryResult(items, DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute ListDischargePointsBySiteQuery");

            return Result<ListDischargePointsBySiteQueryResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargePoint.ListDischargePointsBySiteFailed",
                "Unable to retrieve DischargePoint records at this time.",
                Guid.NewGuid()));
        }
    }
}