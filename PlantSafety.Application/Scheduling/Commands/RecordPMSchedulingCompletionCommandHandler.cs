// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordPMSchedulingCompletionCommand.
// SchemaVersion: 1
// UPDATED: 2026-07-30 -- Two Phase 4 fixes: (1) GetByIdAsync now scoped by request.TenantId,
//   closing a cross-tenant read/mutate gap (usp_PMScheduling_GetById previously filtered by
//   PMSchedulingId only); (2) NotFound ApplicationErrors now pass ErrorSeverity.NotFound
//   explicitly -- the default severity is Error (->500), so a missing record previously returned
//   500, not 404. Also forwards request.ETag to UpdateAsync and lets ConcurrencyConflictException
//   propagate unhandled rather than being swallowed into a generic ApplicationError.

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scheduling.PMScheduling.Interfaces;
using PlantSafety.Domain.Scheduling.PMScheduling.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>Handles <see cref="RecordPMSchedulingCompletionCommand"/>.</summary>
public sealed class RecordPMSchedulingCompletionCommandHandler : IRequestHandler<RecordPMSchedulingCompletionCommand, Result<PMSchedulingActionResult>>
{
    private readonly IPMSchedulingRepository _repository;
    private readonly ILogger<RecordPMSchedulingCompletionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordPMSchedulingCompletionCommandHandler(IPMSchedulingRepository repository, ILogger<RecordPMSchedulingCompletionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PMSchedulingActionResult>> Handle(RecordPMSchedulingCompletionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PMSchedulingId.FromGuid(request.PMSchedulingId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PMSchedulingActionResult>.Failure(new ApplicationError(
                    "Scheduling.PMScheduling.NotFound",
                    "The requested PM scheduling record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordCompletion(
                completionNotes: request.CompletionNotes,
                completedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PMSchedulingActionResult>.Success(new PMSchedulingActionResult(
                aggregate.PMSchedulingId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record completion for PMScheduling {PMSchedulingId}", request.PMSchedulingId);

            return Result<PMSchedulingActionResult>.Failure(new ApplicationError(
                "Scheduling.PMScheduling.RecordCompletionFailed",
                "Unable to record completion at this time.",
                Guid.NewGuid()));
        }
    }
}
