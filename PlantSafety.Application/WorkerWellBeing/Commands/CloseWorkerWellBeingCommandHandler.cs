// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         WorkerWellBeing
//  Layer:          Application
//  File:           CloseWorkerWellBeingCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CloseWorkerWellBeingCommand. Mirrors
//                  ActivateWorkerWellBeingCommandHandler. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.WorkerWellBeing.Repositories;
using PlantSafety.Domain.WorkerWellBeing.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.WorkerWellBeing.Commands;

/// <summary>Handles <see cref="CloseWorkerWellBeingCommand"/>.</summary>
public sealed class CloseWorkerWellBeingCommandHandler : IRequestHandler<CloseWorkerWellBeingCommand, Result<WorkerWellBeingStatusResult>>
{
    private readonly IWorkerWellBeingRepository _repository;
    private readonly ILogger<CloseWorkerWellBeingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseWorkerWellBeingCommandHandler(IWorkerWellBeingRepository repository, ILogger<CloseWorkerWellBeingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkerWellBeingStatusResult>> Handle(CloseWorkerWellBeingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = WorkerWellBeingId.FromGuid(request.WorkerWellBeingId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<WorkerWellBeingStatusResult>.Failure(new ApplicationError(
                    "WorkerWellBeing.WorkerWellBeing.NotFound",
                    "The requested worker wellbeing record could not be found.",
                    request.WorkerWellBeingId,
                    ErrorSeverity.NotFound));
            }

            var actorUserId = UserId.FromGuid(request.ClosedByUserId);

            aggregate.Close(
                actorUserId: actorUserId,
                atUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(actorUserId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<WorkerWellBeingStatusResult>.Success(new WorkerWellBeingStatusResult(
                aggregate.WorkerWellBeingId.Value, aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close WorkerWellBeing {WorkerWellBeingId}", request.WorkerWellBeingId);

            return Result<WorkerWellBeingStatusResult>.Failure(new ApplicationError(
                "WorkerWellBeing.WorkerWellBeing.CloseFailed",
                "Unable to close the worker wellbeing record at this time.",
                Guid.NewGuid()));
        }
    }
}
