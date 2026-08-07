// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for SendCommunityNotificationCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.CommunitySafety.CommunityNotification.Enums;
using PlantSafety.Domain.CommunitySafety.CommunityNotification.Interfaces;
using Agg = PlantSafety.Domain.CommunitySafety.CommunityNotification.Aggregates.CommunityNotification;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.CommunitySafety.Commands;

/// <summary>Handles <see cref="SendCommunityNotificationCommand"/>.</summary>
public sealed class SendCommunityNotificationCommandHandler : IRequestHandler<SendCommunityNotificationCommand, Result<CommunityNotificationActionResult>>
{
    private readonly ICommunityNotificationRepository _repository;
    private readonly ILogger<SendCommunityNotificationCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SendCommunityNotificationCommandHandler(ICommunityNotificationRepository repository, ILogger<SendCommunityNotificationCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityNotificationActionResult>> Handle(SendCommunityNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<CommunityNotificationChannel>(request.Channel, ignoreCase: true, out var channel))
            {
                return Result<CommunityNotificationActionResult>.Failure(new ApplicationError(
                    "CommunitySafety.CommunityNotification.InvalidChannel",
                    $"'{request.Channel}' is not a recognized CommunityNotification channel.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            var aggregate = Agg.Send(
                createdBy: performedBy,
                createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(performedBy),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                channel: channel,
                recipientContactId: request.RecipientContactId,
                subject: request.Subject,
                messageBody: request.MessageBody,
                relatedConcernId: request.RelatedConcernId,
                relatedIncidentId: request.RelatedIncidentId);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CommunityNotificationActionResult>.Success(new CommunityNotificationActionResult(
                aggregate.CommunityNotificationId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send CommunityNotification for Tenant {TenantId}", request.TenantId);

            return Result<CommunityNotificationActionResult>.Failure(new ApplicationError(
                "CommunitySafety.CommunityNotification.SendFailed",
                "Unable to send the CommunityNotification at this time.",
                Guid.NewGuid()));
        }
    }
}
