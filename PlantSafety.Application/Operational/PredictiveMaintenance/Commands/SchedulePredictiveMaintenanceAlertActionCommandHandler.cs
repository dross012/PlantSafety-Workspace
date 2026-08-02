// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           SchedulePredictiveMaintenanceAlertActionCommandHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for SchedulePredictiveMaintenanceAlertActionCommand. Mirrors
//                  ReviewPredictiveMaintenanceAlertCommandHandler's tenant-scoped lookup /
//                  NotFound-severity / concurrency-propagation conventions.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Operational.PredictiveMaintenance.Interfaces;
using PlantSafety.Domain.Operational.PredictiveMaintenance.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Commands;

/// <summary>Handles <see cref="SchedulePredictiveMaintenanceAlertActionCommand"/>.</summary>
public sealed class SchedulePredictiveMaintenanceAlertActionCommandHandler : IRequestHandler<SchedulePredictiveMaintenanceAlertActionCommand, Result<PredictiveMaintenanceAlertStatusResult>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly ILogger<SchedulePredictiveMaintenanceAlertActionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SchedulePredictiveMaintenanceAlertActionCommandHandler(IPredictiveMaintenanceAlertRepository repository, ILogger<SchedulePredictiveMaintenanceAlertActionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PredictiveMaintenanceAlertStatusResult>> Handle(SchedulePredictiveMaintenanceAlertActionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PredictiveMaintenanceAlertId.FromGuid(request.PredictiveMaintenanceAlertId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PredictiveMaintenanceAlertStatusResult>.Failure(new ApplicationError(
                    "Operational.PredictiveMaintenance.NotFound",
                    "The requested predictive maintenance alert could not be found.",
                    request.PredictiveMaintenanceAlertId,
                    ErrorSeverity.NotFound));
            }

            var scheduledBy = UserId.FromGuid(request.ScheduledByUserId);

            aggregate.ScheduleAction(
                scheduledBy: scheduledBy,
                workOrderReference: request.WorkOrderReference,
                scheduledAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(scheduledBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PredictiveMaintenanceAlertStatusResult>.Success(new PredictiveMaintenanceAlertStatusResult(
                aggregate.PredictiveMaintenanceAlertId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule action for PredictiveMaintenanceAlert {PredictiveMaintenanceAlertId}", request.PredictiveMaintenanceAlertId);

            return Result<PredictiveMaintenanceAlertStatusResult>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.ScheduleActionFailed",
                "Unable to schedule corrective action for the predictive maintenance alert at this time.",
                Guid.NewGuid()));
        }
    }
}
