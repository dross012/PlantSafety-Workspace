// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-30
// FILE DESCRIPTION: Handler for GetWorkOrderPriorityByIdQuery. Reads from the read-side repository
//   (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound failure both
//   when the row does not exist and when it belongs to a different tenant, so tenant membership is
//   never revealed to an unauthorized caller. Mirrors
//   PlantSafety.Application.RiskManagement.Handlers.GetRiskByIdQueryHandler.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scheduling.WorkOrderPriority.Models;
using PlantSafety.Application.Scheduling.WorkOrderPriority.Queries;
using PlantSafety.Application.Scheduling.WorkOrderPriority.Repositories;

namespace PlantSafety.Application.Scheduling.WorkOrderPriority.Handlers;

/// <summary>Handles <see cref="GetWorkOrderPriorityByIdQuery"/>.</summary>
public sealed class GetWorkOrderPriorityByIdQueryHandler : IRequestHandler<GetWorkOrderPriorityByIdQuery, Result<WorkOrderPriorityItemDto>>
{
    private readonly IWorkOrderPriorityRepository _repository;
    private readonly ILogger<GetWorkOrderPriorityByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkOrderPriorityByIdQueryHandler(IWorkOrderPriorityRepository repository, ILogger<GetWorkOrderPriorityByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkOrderPriorityItemDto>> Handle(GetWorkOrderPriorityByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.WorkOrderPriorityId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("WorkOrderPriority {WorkOrderPriorityId} not found for Tenant {TenantId}", request.WorkOrderPriorityId, request.TenantId);

                return Result<WorkOrderPriorityItemDto>.Failure(new ApplicationError(
                    "Scheduling.WorkOrderPriority.NotFound",
                    "The requested work order priority record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<WorkOrderPriorityItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve WorkOrderPriority {WorkOrderPriorityId}", request.WorkOrderPriorityId);

            return Result<WorkOrderPriorityItemDto>.Failure(new ApplicationError(
                "Scheduling.WorkOrderPriority.RetrievalFailed",
                "Unable to retrieve work order priority data at this time.",
                Guid.NewGuid()));
        }
    }
}
