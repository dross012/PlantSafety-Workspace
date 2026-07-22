// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for EscalateAlertCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Handles <see cref="EscalateAlertCommand"/>.</summary>
public sealed class EscalateAlertCommandHandler : IRequestHandler<EscalateAlertCommand, Result<EscalateAlertResult>>
{
    private readonly IAlertCommandRepository _repository;
    private readonly ILogger<EscalateAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EscalateAlertCommandHandler(IAlertCommandRepository repository, ILogger<EscalateAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EscalateAlertResult>> Handle(EscalateAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new AlertId(request.AlertId), cancellationToken);
            if (aggregate is null)
            {
                return Result<EscalateAlertResult>.Failure(new ApplicationError(
                    "Alerts.Alert.NotFound", $"Alert '{request.AlertId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            var actor = request.EscalatedByUserId.HasValue
                ? PerformedByActor.User(UserId.FromGuid(request.EscalatedByUserId.Value))
                : PerformedByActor.System();

            aggregate.Escalate(
                request.EscalationReason,
                request.WasAutoEscalated,
                UtcTimestamp.From(request.RequestedAtUtc),
                actor,
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<EscalateAlertResult>.Success(new EscalateAlertResult(aggregate.AlertId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to escalate Alert {AlertId}", request.AlertId);
            return Result<EscalateAlertResult>.Failure(new ApplicationError(
                "Alerts.Alert.EscalateFailed", "Unable to escalate the alert at this time.", request.CorrelationId));
        }
    }
}
