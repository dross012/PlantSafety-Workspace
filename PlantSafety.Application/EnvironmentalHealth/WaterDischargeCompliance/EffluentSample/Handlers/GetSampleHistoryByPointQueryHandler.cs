// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetSampleHistoryByPointQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Repositories;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.Handlers;

/// <summary>Handles <see cref="GetSampleHistoryByPointQuery"/>.</summary>
public sealed class GetSampleHistoryByPointQueryHandler : IRequestHandler<GetSampleHistoryByPointQuery, Result<GetSampleHistoryByPointQueryResult>>
{
    private readonly IEffluentSampleRepository _repository;
    private readonly ILogger<GetSampleHistoryByPointQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSampleHistoryByPointQueryHandler(IEffluentSampleRepository repository, ILogger<GetSampleHistoryByPointQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetSampleHistoryByPointQueryResult>> Handle(GetSampleHistoryByPointQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSampleHistoryByPointAsync(request.TenantId, request.DischargePointId, request.From, request.To, cancellationToken);

            return Result<GetSampleHistoryByPointQueryResult>.Success(new GetSampleHistoryByPointQueryResult(items, DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute GetSampleHistoryByPointQuery");

            return Result<GetSampleHistoryByPointQueryResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.EffluentSample.GetSampleHistoryByPointFailed",
                "Unable to retrieve EffluentSample records at this time.",
                Guid.NewGuid()));
        }
    }
}