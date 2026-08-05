// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for AcknowledgeIncidentNotificationCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Interfaces;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Handles <see cref="AcknowledgeIncidentNotificationCommand"/>.</summary>
public sealed class AcknowledgeIncidentNotificationCommandHandler : IRequestHandler<AcknowledgeIncidentNotificationCommand, Result<IncidentNotificationActionResult>>
{
    private readonly IIncidentNotificationRepository _repository;
    private readonly ILogger<AcknowledgeIncidentNotificationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AcknowledgeIncidentNotificationCommandHandler(IIncidentNotificationRepository repository, ILogger<AcknowledgeIncidentNotificationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentNotificationActionResult>> Handle(AcknowledgeIncidentNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = IncidentNotificationId.FromGuid(request.IncidentNotificationId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<IncidentNotificationActionResult>.Failure(new ApplicationError(
                    "PublicSafetyCommunication.IncidentNotification.NotFound",
                    "The requested incident notification could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var actorId = UserId.FromGuid(request.ActorId);

            aggregate.Acknowledge(
                acknowledgedByContactName: request.AcknowledgedByContactName,
                acknowledgedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(actorId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<IncidentNotificationActionResult>.Success(new IncidentNotificationActionResult(
                aggregate.IncidentNotificationId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge IncidentNotification {IncidentNotificationId}", request.IncidentNotificationId);

            return Result<IncidentNotificationActionResult>.Failure(new ApplicationError(
                "PublicSafetyCommunication.IncidentNotification.AcknowledgeFailed",
                "Unable to acknowledge the incident notification at this time.",
                Guid.NewGuid()));
        }
    }
}
