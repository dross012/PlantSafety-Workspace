// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           UpdateWorkOrderStatusCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for UpdateWorkOrderStatusCommand. Dispatches to the aggregate's
//                  Start/PutOnHold/Resume/Cancel method based on TargetStatus; the aggregate's own
//                  guards (WorkOrderLifecycleRules) enforce which transitions are actually legal
//                  from the current status. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Operational.WorkOrders.Interfaces;
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Application.Operational.WorkOrders.Commands;

/// <summary>Handles <see cref="UpdateWorkOrderStatusCommand"/>.</summary>
public sealed class UpdateWorkOrderStatusCommandHandler : IRequestHandler<UpdateWorkOrderStatusCommand, Result<WorkOrderStatusResult>>
{
    private readonly IWorkOrderRepository _repository;
    private readonly ILogger<UpdateWorkOrderStatusCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public UpdateWorkOrderStatusCommandHandler(IWorkOrderRepository repository, ILogger<UpdateWorkOrderStatusCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkOrderStatusResult>> Handle(UpdateWorkOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = WorkOrderId.FromGuid(request.WorkOrderId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<WorkOrderStatusResult>.Failure(new ApplicationError(
                    "Operational.WorkOrders.NotFound",
                    "The requested work order could not be found.",
                    request.WorkOrderId,
                    ErrorSeverity.NotFound));
            }

            var requesterId = UserId.FromGuid(request.RequestedByUserId);
            var actor = PerformedByActor.User(requesterId);
            var correlationId = new CorrelationId(request.CorrelationId);
            var causationId = new CausationId(request.CausationId);
            var occurredAtUtc = UtcTimestamp.Now();

            switch (request.TargetStatus.Trim().ToLowerInvariant())
            {
                case "start":
                    aggregate.Start(occurredAtUtc, actor, correlationId, causationId);
                    break;
                case "putonhold":
                case "hold":
                    aggregate.PutOnHold(request.Reason, occurredAtUtc, actor, correlationId, causationId);
                    break;
                case "resume":
                    aggregate.Resume(occurredAtUtc, actor, correlationId, causationId);
                    break;
                case "cancel":
                    if (string.IsNullOrWhiteSpace(request.Reason))
                    {
                        return Result<WorkOrderStatusResult>.Failure(new ApplicationError(
                            "Operational.WorkOrders.CancelReasonRequired",
                            "A cancellation reason is required.",
                            request.CorrelationId,
                            ErrorSeverity.Validation));
                    }
                    aggregate.Cancel(request.RequestedByUserId, request.Reason, occurredAtUtc, actor, correlationId, causationId);
                    break;
                default:
                    return Result<WorkOrderStatusResult>.Failure(new ApplicationError(
                        "Operational.WorkOrders.InvalidTargetStatus",
                        $"'{request.TargetStatus}' is not a recognised work order status transition.",
                        request.CorrelationId,
                        ErrorSeverity.Validation));
            }

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<WorkOrderStatusResult>.Success(new WorkOrderStatusResult(
                aggregate.WorkOrderId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update status for WorkOrder {WorkOrderId}", request.WorkOrderId);

            return Result<WorkOrderStatusResult>.Failure(new ApplicationError(
                "Operational.WorkOrders.StatusUpdateFailed",
                "Unable to update the work order status at this time.",
                Guid.NewGuid()));
        }
    }
}
