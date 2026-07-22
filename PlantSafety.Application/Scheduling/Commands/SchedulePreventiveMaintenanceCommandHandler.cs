// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for SchedulePreventiveMaintenanceCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PMScheduling.Interfaces;
using PlantSafety.Domain.Scheduling.PMScheduling.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>Handles <see cref="SchedulePreventiveMaintenanceCommand"/>.</summary>
public sealed class SchedulePreventiveMaintenanceCommandHandler : IRequestHandler<SchedulePreventiveMaintenanceCommand, Result<PMSchedulingActionResult>>
{
    private readonly IPMSchedulingRepository _repository;
    private readonly ILogger<SchedulePreventiveMaintenanceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SchedulePreventiveMaintenanceCommandHandler(IPMSchedulingRepository repository, ILogger<SchedulePreventiveMaintenanceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PMSchedulingActionResult>> Handle(SchedulePreventiveMaintenanceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PMSchedulingId.FromGuid(request.PMSchedulingId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PMSchedulingActionResult>.Failure(new ApplicationError(
                    "Scheduling.PMScheduling.NotFound",
                    "The requested PM scheduling record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.SchedulePreventiveMaintenance(
                dueAtUtc: UtcTimestamp.From(request.DueAtUtc),
                scheduledAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PMSchedulingActionResult>.Success(new PMSchedulingActionResult(
                aggregate.PMSchedulingId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule PMScheduling {PMSchedulingId}", request.PMSchedulingId);

            return Result<PMSchedulingActionResult>.Failure(new ApplicationError(
                "Scheduling.PMScheduling.ScheduleFailed",
                "Unable to schedule the preventive maintenance task at this time.",
                Guid.NewGuid()));
        }
    }
}
