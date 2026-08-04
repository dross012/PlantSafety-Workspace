// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListActiveBroadcastsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Queries;
using PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Repositories;

namespace PlantSafety.Application.PublicSafetyCommunication.PublicAlertBroadcast.Handlers;

/// <summary>Handles <see cref="ListActiveBroadcastsQuery"/>.</summary>
public sealed class ListActiveBroadcastsQueryHandler : IRequestHandler<ListActiveBroadcastsQuery, Result<ListActiveBroadcastsQueryResult>>
{
    private readonly IPublicAlertBroadcastRepository _repository;
    private readonly ILogger<ListActiveBroadcastsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListActiveBroadcastsQueryHandler(IPublicAlertBroadcastRepository repository, ILogger<ListActiveBroadcastsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListActiveBroadcastsQueryResult>> Handle(ListActiveBroadcastsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListActiveBroadcastsAsync(request.TenantId, request.SiteId, cancellationToken);

            return Result<ListActiveBroadcastsQueryResult>.Success(new ListActiveBroadcastsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list active PublicAlertBroadcast records for Tenant {TenantId}", request.TenantId);

            return Result<ListActiveBroadcastsQueryResult>.Failure(new ApplicationError(
                "PublicSafetyCommunication.PublicAlertBroadcast.ListActiveFailed",
                "Unable to retrieve active broadcasts at this time.",
                Guid.NewGuid()));
        }
    }
}
