// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           CompleteWorkOrderCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for CompleteWorkOrderCommand. Additive-only evolution.
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

/// <summary>Handles <see cref="CompleteWorkOrderCommand"/>.</summary>
public sealed class CompleteWorkOrderCommandHandler : IRequestHandler<CompleteWorkOrderCommand, Result<WorkOrderStatusResult>>
{
    private readonly IWorkOrderRepository _repository;
    private readonly ILogger<CompleteWorkOrderCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteWorkOrderCommandHandler(IWorkOrderRepository repository, ILogger<CompleteWorkOrderCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkOrderStatusResult>> Handle(CompleteWorkOrderCommand request, CancellationToken cancellationToken)
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

            aggregate.Complete(
                completedBy: request.RequestedByUserId,
                completionNotes: request.CompletionNotes,
                completedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

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
            _logger.LogError(ex, "Failed to complete WorkOrder {WorkOrderId}", request.WorkOrderId);

            return Result<WorkOrderStatusResult>.Failure(new ApplicationError(
                "Operational.WorkOrders.CompleteFailed",
                "Unable to complete the work order at this time.",
                Guid.NewGuid()));
        }
    }
}
