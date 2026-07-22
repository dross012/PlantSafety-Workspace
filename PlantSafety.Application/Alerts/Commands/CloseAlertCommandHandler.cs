// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for CloseAlertCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Handles <see cref="CloseAlertCommand"/>.</summary>
public sealed class CloseAlertCommandHandler : IRequestHandler<CloseAlertCommand, Result<CloseAlertResult>>
{
    private readonly IAlertCommandRepository _repository;
    private readonly ILogger<CloseAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseAlertCommandHandler(IAlertCommandRepository repository, ILogger<CloseAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CloseAlertResult>> Handle(CloseAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new AlertId(request.AlertId), cancellationToken);
            if (aggregate is null)
            {
                return Result<CloseAlertResult>.Failure(new ApplicationError(
                    "Alerts.Alert.NotFound", $"Alert '{request.AlertId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            aggregate.Close(
                request.CloseOutVerification,
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.ClosedByUserId)),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<CloseAlertResult>.Success(new CloseAlertResult(aggregate.AlertId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to close Alert {AlertId}", request.AlertId);
            return Result<CloseAlertResult>.Failure(new ApplicationError(
                "Alerts.Alert.CloseFailed", "Unable to close the alert at this time.", request.CorrelationId));
        }
    }
}
