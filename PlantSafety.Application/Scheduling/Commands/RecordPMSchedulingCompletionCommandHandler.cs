// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordPMSchedulingCompletionCommand.
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
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PMSchedulingActionResult>.Failure(new ApplicationError(
                    "Scheduling.PMScheduling.NotFound",
                    "The requested PM scheduling record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.RecordCompletion(
                completionNotes: request.CompletionNotes,
                completedAtUtc: UtcTimestamp.Now(),
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
            _logger.LogError(ex, "Failed to record completion for PMScheduling {PMSchedulingId}", request.PMSchedulingId);

            return Result<PMSchedulingActionResult>.Failure(new ApplicationError(
                "Scheduling.PMScheduling.RecordCompletionFailed",
                "Unable to record completion at this time.",
                Guid.NewGuid()));
        }
    }
}
