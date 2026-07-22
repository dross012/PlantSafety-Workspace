// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetWorkOrderPriorityQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.WorkOrderPriority.Queries;
using PlantSafety.Application.Scheduling.WorkOrderPriority.Repositories;

namespace PlantSafety.Application.Scheduling.WorkOrderPriority.Handlers;

/// <summary>Handles <see cref="GetWorkOrderPriorityQuery"/>.</summary>
public sealed class GetWorkOrderPriorityQueryHandler : IRequestHandler<GetWorkOrderPriorityQuery, Result<WorkOrderPriorityQueryResult>>
{
    private readonly IWorkOrderPriorityRepository _repository;
    private readonly ILogger<GetWorkOrderPriorityQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkOrderPriorityQueryHandler(IWorkOrderPriorityRepository repository, ILogger<GetWorkOrderPriorityQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkOrderPriorityQueryResult>> Handle(GetWorkOrderPriorityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetWorkOrderPriorityAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<WorkOrderPriorityQueryResult>.Success(new WorkOrderPriorityQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve WorkOrderPriority for Tenant {TenantId}", request.TenantId);

            return Result<WorkOrderPriorityQueryResult>.Failure(new ApplicationError(
                "Scheduling.WorkOrderPriority.RetrievalFailed",
                "Unable to retrieve WorkOrderPriority data at this time.",
                Guid.NewGuid()));
        }
    }
}