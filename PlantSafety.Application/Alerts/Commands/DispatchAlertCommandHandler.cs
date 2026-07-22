// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for DispatchAlertCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Handles <see cref="DispatchAlertCommand"/>.</summary>
public sealed class DispatchAlertCommandHandler : IRequestHandler<DispatchAlertCommand, Result<DispatchAlertResult>>
{
    private readonly IAlertCommandRepository _repository;
    private readonly ILogger<DispatchAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DispatchAlertCommandHandler(IAlertCommandRepository repository, ILogger<DispatchAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DispatchAlertResult>> Handle(DispatchAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new AlertId(request.AlertId), cancellationToken);
            if (aggregate is null)
            {
                return Result<DispatchAlertResult>.Failure(new ApplicationError(
                    "Alerts.Alert.NotFound", $"Alert '{request.AlertId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            aggregate.Dispatch(
                request.DispatchedTo,
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.DispatchedByUserId)),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<DispatchAlertResult>.Success(new DispatchAlertResult(aggregate.AlertId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch response for Alert {AlertId}", request.AlertId);
            return Result<DispatchAlertResult>.Failure(new ApplicationError(
                "Alerts.Alert.DispatchFailed", "Unable to dispatch a response for the alert at this time.", request.CorrelationId));
        }
    }
}
