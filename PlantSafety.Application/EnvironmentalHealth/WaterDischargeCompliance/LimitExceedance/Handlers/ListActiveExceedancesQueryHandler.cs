// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListActiveExceedancesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Repositories;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.Handlers;

/// <summary>Handles <see cref="ListActiveExceedancesQuery"/>.</summary>
public sealed class ListActiveExceedancesQueryHandler : IRequestHandler<ListActiveExceedancesQuery, Result<ListActiveExceedancesQueryResult>>
{
    private readonly ILimitExceedanceRepository _repository;
    private readonly ILogger<ListActiveExceedancesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListActiveExceedancesQueryHandler(ILimitExceedanceRepository repository, ILogger<ListActiveExceedancesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListActiveExceedancesQueryResult>> Handle(ListActiveExceedancesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListActiveExceedancesAsync(request.TenantId, request.SiteId, cancellationToken);

            return Result<ListActiveExceedancesQueryResult>.Success(new ListActiveExceedancesQueryResult(items, DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute ListActiveExceedancesQuery");

            return Result<ListActiveExceedancesQueryResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.LimitExceedance.ListActiveExceedancesFailed",
                "Unable to retrieve LimitExceedance records at this time.",
                Guid.NewGuid()));
        }
    }
}