// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetTrackingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Tracking.Queries;
using PlantSafety.Application.Compliance.Tracking.Repositories;

namespace PlantSafety.Application.Compliance.Tracking.Handlers;

/// <summary>Handles <see cref="GetTrackingQuery"/>.</summary>
public sealed class GetTrackingQueryHandler : IRequestHandler<GetTrackingQuery, Result<TrackingQueryResult>>
{
    private readonly ITrackingRepository _repository;
    private readonly ILogger<GetTrackingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetTrackingQueryHandler(ITrackingRepository repository, ILogger<GetTrackingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TrackingQueryResult>> Handle(GetTrackingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetTrackingAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<TrackingQueryResult>.Success(new TrackingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Tracking for Tenant {TenantId}", request.TenantId);

            return Result<TrackingQueryResult>.Failure(new ApplicationError(
                "Compliance.Tracking.RetrievalFailed",
                "Unable to retrieve compliance tracking data at this time.",
                Guid.NewGuid()));
        }
    }
}
