// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for RecordAlertDeliveryCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Alerts.Alert.Enums;
using PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Handles <see cref="RecordAlertDeliveryCommand"/>.</summary>
public sealed class RecordAlertDeliveryCommandHandler : IRequestHandler<RecordAlertDeliveryCommand, Result<RecordAlertDeliveryResult>>
{
    private readonly IAlertCommandRepository _repository;
    private readonly ILogger<RecordAlertDeliveryCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordAlertDeliveryCommandHandler(IAlertCommandRepository repository, ILogger<RecordAlertDeliveryCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordAlertDeliveryResult>> Handle(RecordAlertDeliveryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<AlertDeliveryChannel>(request.ChannelCode, ignoreCase: true, out var channel))
            {
                return Result<RecordAlertDeliveryResult>.Failure(new ApplicationError(
                    "Alerts.Alert.InvalidDeliveryChannel", $"Unknown delivery channel '{request.ChannelCode}'.", request.CorrelationId, ErrorSeverity.Validation));
            }

            var aggregate = await _repository.GetByIdAsync(new AlertId(request.AlertId), cancellationToken);
            if (aggregate is null)
            {
                return Result<RecordAlertDeliveryResult>.Failure(new ApplicationError(
                    "Alerts.Alert.NotFound", $"Alert '{request.AlertId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            var actor = request.RecordedByUserId.HasValue
                ? PerformedByActor.User(UserId.FromGuid(request.RecordedByUserId.Value))
                : PerformedByActor.System();

            aggregate.RecordDelivery(
                Guid.NewGuid(),
                channel,
                request.Succeeded,
                UtcTimestamp.From(request.RequestedAtUtc),
                actor,
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New(),
                request.FailureReason);

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<RecordAlertDeliveryResult>.Success(new RecordAlertDeliveryResult(aggregate.AlertId.Value, channel.ToString(), request.Succeeded));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record delivery for Alert {AlertId}", request.AlertId);
            return Result<RecordAlertDeliveryResult>.Failure(new ApplicationError(
                "Alerts.Alert.RecordDeliveryFailed", "Unable to record the delivery attempt at this time.", request.CorrelationId));
        }
    }
}
