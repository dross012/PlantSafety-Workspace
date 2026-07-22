// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for AcknowledgeAlertCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Handles <see cref="AcknowledgeAlertCommand"/>.</summary>
public sealed class AcknowledgeAlertCommandHandler : IRequestHandler<AcknowledgeAlertCommand, Result<AcknowledgeAlertResult>>
{
    private readonly IAlertCommandRepository _repository;
    private readonly ILogger<AcknowledgeAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AcknowledgeAlertCommandHandler(IAlertCommandRepository repository, ILogger<AcknowledgeAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AcknowledgeAlertResult>> Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var alertId = new AlertId(request.AlertId);
            var aggregate = await _repository.GetByIdAsync(alertId, cancellationToken);

            if (aggregate is null)
            {
                return Result<AcknowledgeAlertResult>.Failure(new ApplicationError(
                    "Alerts.Alert.NotFound",
                    $"Alert '{request.AlertId}' was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var correlationId = new CorrelationId(request.CorrelationId);
            var causationId = request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New();

            aggregate.Acknowledge(
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.AcknowledgedByUserId)),
                correlationId,
                causationId);

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<AcknowledgeAlertResult>.Success(new AcknowledgeAlertResult(aggregate.AlertId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge Alert {AlertId}", request.AlertId);

            return Result<AcknowledgeAlertResult>.Failure(new ApplicationError(
                "Alerts.Alert.AcknowledgeFailed",
                "Unable to acknowledge the alert at this time.",
                request.CorrelationId));
        }
    }
}
