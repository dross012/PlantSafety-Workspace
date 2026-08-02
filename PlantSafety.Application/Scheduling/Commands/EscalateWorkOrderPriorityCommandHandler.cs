// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for EscalateWorkOrderPriorityCommand.
// SchemaVersion: 1
// UPDATED: 2026-07-30 -- Two Phase 4 fixes: (1) GetByIdAsync now scoped by request.TenantId,
//   closing a cross-tenant read/mutate gap (usp_WorkOrderPriority_GetById previously filtered by
//   WorkOrderPriorityId only); (2) NotFound ApplicationErrors now pass ErrorSeverity.NotFound
//   explicitly -- the default severity is Error (->500), so a missing record previously returned
//   500, not 404. Also forwards request.ETag to UpdateAsync and lets ConcurrencyConflictException
//   propagate unhandled rather than being swallowed into a generic ApplicationError.

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.WorkOrderPriority.Interfaces;
using PlantSafety.Domain.Scheduling.WorkOrderPriority.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>Handles <see cref="EscalateWorkOrderPriorityCommand"/>.</summary>
public sealed class EscalateWorkOrderPriorityCommandHandler : IRequestHandler<EscalateWorkOrderPriorityCommand, Result<WorkOrderPriorityActionResult>>
{
    private readonly IWorkOrderPriorityRepository _repository;
    private readonly ILogger<EscalateWorkOrderPriorityCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EscalateWorkOrderPriorityCommandHandler(IWorkOrderPriorityRepository repository, ILogger<EscalateWorkOrderPriorityCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkOrderPriorityActionResult>> Handle(EscalateWorkOrderPriorityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = WorkOrderPriorityId.FromGuid(request.WorkOrderPriorityId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<WorkOrderPriorityActionResult>.Failure(new ApplicationError(
                    "Scheduling.WorkOrderPriority.NotFound",
                    "The requested work order priority record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Escalate(
                escalationReason: request.EscalationReason,
                escalatedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<WorkOrderPriorityActionResult>.Success(new WorkOrderPriorityActionResult(
                aggregate.WorkOrderPriorityId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to escalate WorkOrderPriority {WorkOrderPriorityId}", request.WorkOrderPriorityId);

            return Result<WorkOrderPriorityActionResult>.Failure(new ApplicationError(
                "Scheduling.WorkOrderPriority.EscalateFailed",
                "Unable to escalate the work order at this time.",
                Guid.NewGuid()));
        }
    }
}
