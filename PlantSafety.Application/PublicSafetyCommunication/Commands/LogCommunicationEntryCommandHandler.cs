// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for LogCommunicationEntryCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Aggregates;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.CommunicationLog.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Handles <see cref="LogCommunicationEntryCommand"/>.</summary>
public sealed class LogCommunicationEntryCommandHandler : IRequestHandler<LogCommunicationEntryCommand, Result<CommunicationLogEntryActionResult>>
{
    private readonly ICommunicationLogEntryRepository _repository;
    private readonly ILogger<LogCommunicationEntryCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public LogCommunicationEntryCommandHandler(ICommunicationLogEntryRepository repository, ILogger<LogCommunicationEntryCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunicationLogEntryActionResult>> Handle(LogCommunicationEntryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<CommunicationDirection>(request.Direction, ignoreCase: true, out var direction))
            {
                return Result<CommunicationLogEntryActionResult>.Failure(new ApplicationError(
                    "PublicSafetyCommunication.CommunicationLogEntry.InvalidDirection",
                    $"'{request.Direction}' is not a recognised CommunicationDirection.",
                    Guid.NewGuid()));
            }

            if (!Enum.TryParse<CommunicationMedium>(request.Medium, ignoreCase: true, out var medium))
            {
                return Result<CommunicationLogEntryActionResult>.Failure(new ApplicationError(
                    "PublicSafetyCommunication.CommunicationLogEntry.InvalidMedium",
                    $"'{request.Medium}' is not a recognised CommunicationMedium.",
                    Guid.NewGuid()));
            }

            var actorId = UserId.FromGuid(request.ActorId);

            var aggregate = CommunicationLogEntry.Create(
                createdBy: actorId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(actorId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                incidentId: request.IncidentId,
                relatedIncidentNotificationId: request.RelatedIncidentNotificationId,
                relatedPublicAlertBroadcastId: request.RelatedPublicAlertBroadcastId,
                direction: direction,
                medium: medium,
                counterpartyName: request.CounterpartyName,
                summary: request.Summary,
                occurredAtUtc: UtcTimestamp.From(request.OccurredAtUtc));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CommunicationLogEntryActionResult>.Success(new CommunicationLogEntryActionResult(
                aggregate.CommunicationLogEntryId.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log CommunicationLogEntry for incident {IncidentId}", request.IncidentId);

            return Result<CommunicationLogEntryActionResult>.Failure(new ApplicationError(
                "PublicSafetyCommunication.CommunicationLogEntry.LogFailed",
                "Unable to log the communication entry at this time.",
                Guid.NewGuid()));
        }
    }
}
