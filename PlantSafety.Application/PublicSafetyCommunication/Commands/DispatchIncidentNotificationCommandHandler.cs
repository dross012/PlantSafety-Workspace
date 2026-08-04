// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for DispatchIncidentNotificationCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.ValueObjects.Ids;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Handles <see cref="DispatchIncidentNotificationCommand"/>.</summary>
public sealed class DispatchIncidentNotificationCommandHandler : IRequestHandler<DispatchIncidentNotificationCommand, Result<IncidentNotificationActionResult>>
{
    private readonly IIncidentNotificationRepository _repository;
    private readonly ILogger<DispatchIncidentNotificationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DispatchIncidentNotificationCommandHandler(IIncidentNotificationRepository repository, ILogger<DispatchIncidentNotificationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentNotificationActionResult>> Handle(DispatchIncidentNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!PublicSafetyDeliveryChannel.TryParse(request.DeliveryChannelCode, out var channel))
            {
                return Result<IncidentNotificationActionResult>.Failure(new ApplicationError(
                    "PublicSafetyCommunication.IncidentNotification.InvalidChannel",
                    $"'{request.DeliveryChannelCode}' is not a recognised PublicSafetyDeliveryChannel.",
                    Guid.NewGuid()));
            }

            var actorId = UserId.FromGuid(request.ActorId);

            var aggregate = IncidentNotification.Create(
                createdBy: actorId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(actorId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                incidentId: request.IncidentId,
                publicSafetyAgencyId: PublicSafetyAgencyId.FromGuid(request.PublicSafetyAgencyId),
                channel: channel!,
                message: request.Message);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<IncidentNotificationActionResult>.Success(new IncidentNotificationActionResult(
                aggregate.IncidentNotificationId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch IncidentNotification to agency {PublicSafetyAgencyId}", request.PublicSafetyAgencyId);

            return Result<IncidentNotificationActionResult>.Failure(new ApplicationError(
                "PublicSafetyCommunication.IncidentNotification.DispatchFailed",
                "Unable to dispatch the incident notification at this time.",
                Guid.NewGuid()));
        }
    }
}
