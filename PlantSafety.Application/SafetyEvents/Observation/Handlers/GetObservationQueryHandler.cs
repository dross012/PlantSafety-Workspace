// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetObservationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.Observation.Queries;
using PlantSafety.Application.SafetyEvents.Observation.Repositories;

namespace PlantSafety.Application.SafetyEvents.Observation.Handlers;

/// <summary>Handles <see cref="GetObservationQuery"/>.</summary>
public sealed class GetObservationQueryHandler : IRequestHandler<GetObservationQuery, Result<ObservationQueryResult>>
{
    private readonly IObservationRepository _repository;
    private readonly ILogger<GetObservationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetObservationQueryHandler(IObservationRepository repository, ILogger<GetObservationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ObservationQueryResult>> Handle(GetObservationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetObservationsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ObservationQueryResult>.Success(new ObservationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Observation for Tenant {TenantId}", request.TenantId);

            return Result<ObservationQueryResult>.Failure(new ApplicationError(
                "SafetyEvents.Observation.RetrievalFailed",
                "Unable to retrieve observation data at this time.",
                Guid.NewGuid()));
        }
    }
}
