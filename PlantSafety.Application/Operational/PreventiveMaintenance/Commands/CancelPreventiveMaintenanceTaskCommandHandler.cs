// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PreventiveMaintenance
//  Layer:          Application
//  File:           CancelPreventiveMaintenanceTaskCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for CancelPreventiveMaintenanceTaskCommand. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.RejectPermitsCommandHandler.
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

/// <summary>Handles <see cref="CancelPreventiveMaintenanceTaskCommand"/>.</summary>
public sealed class CancelPreventiveMaintenanceTaskCommandHandler : IRequestHandler<CancelPreventiveMaintenanceTaskCommand, Result<PreventiveMaintenanceTaskStatusResult>>
{
    private readonly IPreventiveMaintenanceTaskRepository _repository;
    private readonly ILogger<CancelPreventiveMaintenanceTaskCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CancelPreventiveMaintenanceTaskCommandHandler(IPreventiveMaintenanceTaskRepository repository, ILogger<CancelPreventiveMaintenanceTaskCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PreventiveMaintenanceTaskStatusResult>> Handle(CancelPreventiveMaintenanceTaskCommand request, CancellationToken cancellationToken)
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

            var cancelledBy = UserId.FromGuid(request.CancelledByUserId);

            aggregate.Cancel(
                cancelledBy: cancelledBy,
                reason: request.Reason,
                cancelledAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(cancelledBy),
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
            _logger.LogError(ex, "Failed to cancel PreventiveMaintenanceTask {PreventiveMaintenanceTaskId}", request.PreventiveMaintenanceTaskId);

            return Result<PreventiveMaintenanceTaskStatusResult>.Failure(new ApplicationError(
                "Operational.PreventiveMaintenance.CancelFailed",
                "Unable to cancel the preventive maintenance task at this time.",
                Guid.NewGuid()));
        }
    }
}
