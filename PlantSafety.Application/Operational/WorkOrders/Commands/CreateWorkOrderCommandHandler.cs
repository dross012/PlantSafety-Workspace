// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           CreateWorkOrderCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateWorkOrderCommand. Pure orchestration: constructs the
//                  WorkOrder aggregate via its domain factory method and persists it through the
//                  domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Operational.PreventiveMaintenance.Commands
//                  .SchedulePreventiveMaintenanceTaskCommandHandler. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Operational.WorkOrders.Enums;
using PlantSafety.Domain.Operational.WorkOrders.Interfaces;

namespace PlantSafety.Application.Operational.WorkOrders.Commands;

/// <summary>Handles <see cref="CreateWorkOrderCommand"/>.</summary>
public sealed class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, Result<CreateWorkOrderResult>>
{
    private readonly IWorkOrderRepository _repository;
    private readonly ILogger<CreateWorkOrderCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateWorkOrderCommandHandler(IWorkOrderRepository repository, ILogger<CreateWorkOrderCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateWorkOrderResult>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            if (!Enum.TryParse<WorkOrderPriorityLevel>(request.Priority, ignoreCase: true, out var priority))
            {
                return Result<CreateWorkOrderResult>.Failure(new ApplicationError(
                    "Operational.WorkOrders.InvalidPriority",
                    $"'{request.Priority}' is not a recognised work order priority.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var aggregate = PlantSafety.Domain.Operational.WorkOrders.Aggregates.WorkOrder.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                workOrderCode: request.WorkOrderCode,
                machineId: request.MachineId,
                machineCode: request.MachineCode,
                priority: priority,
                description: request.Description,
                dueAtUtc: UtcTimestamp.From(request.DueAtUtc));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateWorkOrderResult>.Success(new CreateWorkOrderResult(
                aggregate.WorkOrderId.Value,
                aggregate.WorkOrderCode,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create WorkOrder for Tenant {TenantId}", request.TenantId);

            return Result<CreateWorkOrderResult>.Failure(new ApplicationError(
                "Operational.WorkOrders.CreateFailed",
                "Unable to create the work order at this time.",
                Guid.NewGuid()));
        }
    }
}
