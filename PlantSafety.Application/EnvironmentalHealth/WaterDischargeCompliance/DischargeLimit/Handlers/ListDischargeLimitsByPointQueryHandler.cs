// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListDischargeLimitsByPointQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Repositories;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.Handlers;

/// <summary>Handles <see cref="ListDischargeLimitsByPointQuery"/>.</summary>
public sealed class ListDischargeLimitsByPointQueryHandler : IRequestHandler<ListDischargeLimitsByPointQuery, Result<ListDischargeLimitsByPointQueryResult>>
{
    private readonly IDischargeLimitRepository _repository;
    private readonly ILogger<ListDischargeLimitsByPointQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListDischargeLimitsByPointQueryHandler(IDischargeLimitRepository repository, ILogger<ListDischargeLimitsByPointQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListDischargeLimitsByPointQueryResult>> Handle(ListDischargeLimitsByPointQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListDischargeLimitsByPointAsync(request.TenantId, request.DischargePointId, cancellationToken);

            return Result<ListDischargeLimitsByPointQueryResult>.Success(new ListDischargeLimitsByPointQueryResult(items, DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute ListDischargeLimitsByPointQuery");

            return Result<ListDischargeLimitsByPointQueryResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargeLimit.ListDischargeLimitsByPointFailed",
                "Unable to retrieve DischargeLimit records at this time.",
                Guid.NewGuid()));
        }
    }
}