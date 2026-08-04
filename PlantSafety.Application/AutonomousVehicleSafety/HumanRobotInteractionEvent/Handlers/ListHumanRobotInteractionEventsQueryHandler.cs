// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListHumanRobotInteractionEventsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Queries;
using PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Repositories;

namespace PlantSafety.Application.AutonomousVehicleSafety.HumanRobotInteractionEvent.Handlers;

/// <summary>Handles <see cref="ListHumanRobotInteractionEventsQuery"/>.</summary>
public sealed class ListHumanRobotInteractionEventsQueryHandler : IRequestHandler<ListHumanRobotInteractionEventsQuery, Result<HumanRobotInteractionEventQueryResult>>
{
    private readonly IHumanRobotInteractionEventRepository _repository;
    private readonly ILogger<ListHumanRobotInteractionEventsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListHumanRobotInteractionEventsQueryHandler(IHumanRobotInteractionEventRepository repository, ILogger<ListHumanRobotInteractionEventsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HumanRobotInteractionEventQueryResult>> Handle(ListHumanRobotInteractionEventsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHumanRobotInteractionEventItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HumanRobotInteractionEventQueryResult>.Success(new HumanRobotInteractionEventQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HumanRobotInteractionEvent records for Tenant {TenantId}", request.TenantId);

            return Result<HumanRobotInteractionEventQueryResult>.Failure(new ApplicationError(
                "AutonomousVehicleSafety.HumanRobotInteractionEvent.RetrievalFailed",
                "Unable to retrieve HumanRobotInteractionEvent records at this time.",
                Guid.NewGuid()));
        }
    }
}
