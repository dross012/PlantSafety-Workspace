// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           MarkPreventiveMaintenanceTaskOverdueCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for MarkPreventiveMaintenanceTaskOverdueCommand. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.ExpirePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Operational.PreventiveMaintenance.Interfaces;
using PlantSafety.Domain.Operational.PreventiveMaintenance.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PreventiveMaintenance.Commands;

/// <summary>Handles <see cref="MarkPreventiveMaintenanceTaskOverdueCommand"/>.</summary>
public sealed class MarkPreventiveMaintenanceTaskOverdueCommandHandler : IRequestHandler<MarkPreventiveMaintenanceTaskOverdueCommand, Result<PreventiveMaintenanceTaskStatusResult>>
{
    private readonly IPreventiveMaintenanceTaskRepository _repository;
    private readonly ILogger<MarkPreventiveMaintenanceTaskOverdueCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public MarkPreventiveMaintenanceTaskOverdueCommandHandler(IPreventiveMaintenanceTaskRepository repository, ILogger<MarkPreventiveMaintenanceTaskOverdueCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PreventiveMaintenanceTaskStatusResult>> Handle(MarkPreventiveMaintenanceTaskOverdueCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PreventiveMaintenanceTaskId.FromGuid(request.PreventiveMaintenanceTaskId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PreventiveMaintenanceTaskStatusResult>.Failure(new ApplicationError(
                    "Operational.PreventiveMaintenance.NotFound",
                    "The requested preventive maintenance task could not be found.",
                    request.PreventiveMaintenanceTaskId,
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.MarkOverdue(
                overdueAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PreventiveMaintenanceTaskStatusResult>.Success(new PreventiveMaintenanceTaskStatusResult(
                aggregate.PreventiveMaintenanceTaskId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark PreventiveMaintenanceTask {PreventiveMaintenanceTaskId} overdue", request.PreventiveMaintenanceTaskId);

            return Result<PreventiveMaintenanceTaskStatusResult>.Failure(new ApplicationError(
                "Operational.PreventiveMaintenance.MarkOverdueFailed",
                "Unable to mark the preventive maintenance task overdue at this time.",
                Guid.NewGuid()));
        }
    }
}
