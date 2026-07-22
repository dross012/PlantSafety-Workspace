// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for RecordAlertResponseActionCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Handles <see cref="RecordAlertResponseActionCommand"/>.</summary>
public sealed class RecordAlertResponseActionCommandHandler : IRequestHandler<RecordAlertResponseActionCommand, Result<RecordAlertResponseActionResult>>
{
    private readonly IAlertCommandRepository _repository;
    private readonly ILogger<RecordAlertResponseActionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordAlertResponseActionCommandHandler(IAlertCommandRepository repository, ILogger<RecordAlertResponseActionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordAlertResponseActionResult>> Handle(RecordAlertResponseActionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new AlertId(request.AlertId), cancellationToken);
            if (aggregate is null)
            {
                return Result<RecordAlertResponseActionResult>.Failure(new ApplicationError(
                    "Alerts.Alert.NotFound", $"Alert '{request.AlertId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            var responseActionId = Guid.NewGuid();

            aggregate.RecordResponseAction(
                responseActionId,
                request.ActionDescription,
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.TakenByUserId)),
                request.Notes);

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<RecordAlertResponseActionResult>.Success(new RecordAlertResponseActionResult(aggregate.AlertId.Value, responseActionId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record response action for Alert {AlertId}", request.AlertId);
            return Result<RecordAlertResponseActionResult>.Failure(new ApplicationError(
                "Alerts.Alert.RecordResponseActionFailed", "Unable to record the response action at this time.", request.CorrelationId));
        }
    }
}
