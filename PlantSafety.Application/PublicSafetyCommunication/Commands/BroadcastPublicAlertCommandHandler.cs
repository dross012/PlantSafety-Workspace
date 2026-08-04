// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for BroadcastPublicAlertCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Interfaces;
using PublicAlertBroadcastAggregate = PlantSafety.Domain.PublicSafetyCommunication.PublicAlertBroadcast.Aggregates.PublicAlertBroadcast;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Handles <see cref="BroadcastPublicAlertCommand"/>.</summary>
public sealed class BroadcastPublicAlertCommandHandler : IRequestHandler<BroadcastPublicAlertCommand, Result<PublicAlertBroadcastActionResult>>
{
    private readonly IPublicAlertBroadcastRepository _repository;
    private readonly ILogger<BroadcastPublicAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BroadcastPublicAlertCommandHandler(IPublicAlertBroadcastRepository repository, ILogger<BroadcastPublicAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PublicAlertBroadcastActionResult>> Handle(BroadcastPublicAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!PublicAlertSeverity.TryParse(request.SeverityCode, out var severity))
            {
                return Result<PublicAlertBroadcastActionResult>.Failure(new ApplicationError(
                    "PublicSafetyCommunication.PublicAlertBroadcast.InvalidSeverity",
                    $"'{request.SeverityCode}' is not a recognised PublicAlertSeverity.",
                    Guid.NewGuid()));
            }

            if (!PublicBroadcastChannel.TryParse(request.ChannelCode, out var channel))
            {
                return Result<PublicAlertBroadcastActionResult>.Failure(new ApplicationError(
                    "PublicSafetyCommunication.PublicAlertBroadcast.InvalidChannel",
                    $"'{request.ChannelCode}' is not a recognised PublicBroadcastChannel.",
                    Guid.NewGuid()));
            }

            var actorId = UserId.FromGuid(request.ActorId);

            var aggregate = PublicAlertBroadcastAggregate.Create(
                createdBy: actorId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(actorId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                incidentId: request.IncidentId,
                severity: severity!,
                channel: channel!,
                headline: request.Headline,
                message: request.Message,
                targetArea: request.TargetArea);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<PublicAlertBroadcastActionResult>.Success(new PublicAlertBroadcastActionResult(
                aggregate.PublicAlertBroadcastId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send PublicAlertBroadcast for incident {IncidentId}", request.IncidentId);

            return Result<PublicAlertBroadcastActionResult>.Failure(new ApplicationError(
                "PublicSafetyCommunication.PublicAlertBroadcast.BroadcastFailed",
                "Unable to send the public alert broadcast at this time.",
                Guid.NewGuid()));
        }
    }
}
