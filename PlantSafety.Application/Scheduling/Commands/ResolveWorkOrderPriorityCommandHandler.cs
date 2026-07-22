// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ResolveWorkOrderPriorityCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.WorkOrderPriority.Interfaces;
using PlantSafety.Domain.Scheduling.WorkOrderPriority.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>Handles <see cref="ResolveWorkOrderPriorityCommand"/>.</summary>
public sealed class ResolveWorkOrderPriorityCommandHandler : IRequestHandler<ResolveWorkOrderPriorityCommand, Result<WorkOrderPriorityActionResult>>
{
    private readonly IWorkOrderPriorityRepository _repository;
    private readonly ILogger<ResolveWorkOrderPriorityCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ResolveWorkOrderPriorityCommandHandler(IWorkOrderPriorityRepository repository, ILogger<ResolveWorkOrderPriorityCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkOrderPriorityActionResult>> Handle(ResolveWorkOrderPriorityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = WorkOrderPriorityId.FromGuid(request.WorkOrderPriorityId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<WorkOrderPriorityActionResult>.Failure(new ApplicationError(
                    "Scheduling.WorkOrderPriority.NotFound",
                    "The requested work order priority record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Resolve(
                resolutionNotes: request.ResolutionNotes,
                resolvedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<WorkOrderPriorityActionResult>.Success(new WorkOrderPriorityActionResult(
                aggregate.WorkOrderPriorityId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve WorkOrderPriority {WorkOrderPriorityId}", request.WorkOrderPriorityId);

            return Result<WorkOrderPriorityActionResult>.Failure(new ApplicationError(
                "Scheduling.WorkOrderPriority.ResolveFailed",
                "Unable to resolve the work order at this time.",
                Guid.NewGuid()));
        }
    }
}
