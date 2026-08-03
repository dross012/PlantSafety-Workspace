// AUTHOR: PlantSafety Platform Team (Eventing & Messaging Agent, Phase 6)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for ReplayDeadLetterEventCommand.
// SchemaVersion: 1

using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Messaging.Commands;
using PlantSafety.Application.Messaging.Repositories;

namespace PlantSafety.Application.Messaging.Handlers;

/// <summary>Handles <see cref="ReplayDeadLetterEventCommand"/>.</summary>
public sealed class ReplayDeadLetterEventCommandHandler : IRequestHandler<ReplayDeadLetterEventCommand, Result<bool>>
{
    private readonly IDeadLetterEventRepository _repository;
    private readonly ILogger<ReplayDeadLetterEventCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ReplayDeadLetterEventCommandHandler(
        IDeadLetterEventRepository repository, ILogger<ReplayDeadLetterEventCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> Handle(ReplayDeadLetterEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var replayed = await _repository.ReplayAsync(request.DeadLetterId, request.ActorId, cancellationToken);

            if (replayed)
            {
                _logger.LogInformation(
                    "ReplayDeadLetterEventCommand: DeadLetterId={DeadLetterId} replayed by ActorId={ActorId}.",
                    request.DeadLetterId, request.ActorId);
            }
            else
            {
                _logger.LogWarning(
                    "ReplayDeadLetterEventCommand: DeadLetterId={DeadLetterId} not found or already replayed.",
                    request.DeadLetterId);
            }

            return Result<bool>.Success(replayed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to replay dead-lettered event {DeadLetterId}.", request.DeadLetterId);

            return Result<bool>.Failure(new ApplicationError(
                "Messaging.DeadLetterEvents.ReplayFailed",
                "Unable to replay this dead-lettered event at this time.",
                request.CorrelationId));
        }
    }
}
