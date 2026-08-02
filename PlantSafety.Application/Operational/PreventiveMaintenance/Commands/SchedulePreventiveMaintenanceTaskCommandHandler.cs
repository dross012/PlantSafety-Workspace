// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           SchedulePreventiveMaintenanceTaskCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for SchedulePreventiveMaintenanceTaskCommand. Pure orchestration:
//                  constructs the PreventiveMaintenanceTask aggregate via its domain factory
//                  method and persists it through the domain repository interface. No business
//                  rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Commands;

/// <summary>Handles <see cref="SchedulePreventiveMaintenanceTaskCommand"/>.</summary>
public sealed class SchedulePreventiveMaintenanceTaskCommandHandler : IRequestHandler<SchedulePreventiveMaintenanceTaskCommand, Result<SchedulePreventiveMaintenanceTaskResult>>
{
    private readonly IPreventiveMaintenanceTaskRepository _repository;
    private readonly ILogger<SchedulePreventiveMaintenanceTaskCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SchedulePreventiveMaintenanceTaskCommandHandler(IPreventiveMaintenanceTaskRepository repository, ILogger<SchedulePreventiveMaintenanceTaskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SchedulePreventiveMaintenanceTaskResult>> Handle(SchedulePreventiveMaintenanceTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            var aggregate = PlantSafety.Domain.Operational.PreventiveMaintenance.Aggregates.PreventiveMaintenanceTask.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                equipmentId: request.EquipmentId,
                taskDescription: request.TaskDescription,
                scheduledAtUtc: UtcTimestamp.From(request.ScheduledAtUtc));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<SchedulePreventiveMaintenanceTaskResult>.Success(new SchedulePreventiveMaintenanceTaskResult(
                aggregate.PreventiveMaintenanceTaskId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule PreventiveMaintenanceTask for Tenant {TenantId}", request.TenantId);

            return Result<SchedulePreventiveMaintenanceTaskResult>.Failure(new ApplicationError(
                "Operational.PreventiveMaintenance.ScheduleFailed",
                "Unable to schedule the preventive maintenance task at this time.",
                Guid.NewGuid()));
        }
    }
}
