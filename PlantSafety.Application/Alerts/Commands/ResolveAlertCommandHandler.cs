// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for ResolveAlertCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Alerts.Alert.ValueObjects.Ids;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Handles <see cref="ResolveAlertCommand"/>.</summary>
public sealed class ResolveAlertCommandHandler : IRequestHandler<ResolveAlertCommand, Result<ResolveAlertResult>>
{
    private readonly IAlertCommandRepository _repository;
    private readonly ILogger<ResolveAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ResolveAlertCommandHandler(IAlertCommandRepository repository, ILogger<ResolveAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ResolveAlertResult>> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var aggregate = await _repository.GetByIdAsync(new AlertId(request.AlertId), cancellationToken);
            if (aggregate is null)
            {
                return Result<ResolveAlertResult>.Failure(new ApplicationError(
                    "Alerts.Alert.NotFound", $"Alert '{request.AlertId}' was not found.", request.CorrelationId, ErrorSeverity.NotFound));
            }

            aggregate.Resolve(
                request.ResponseActionsTaken,
                UtcTimestamp.From(request.RequestedAtUtc),
                PerformedByActor.User(UserId.FromGuid(request.ResolvedByUserId)),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<ResolveAlertResult>.Success(new ResolveAlertResult(aggregate.AlertId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve Alert {AlertId}", request.AlertId);
            return Result<ResolveAlertResult>.Failure(new ApplicationError(
                "Alerts.Alert.ResolveFailed", "Unable to resolve the alert at this time.", request.CorrelationId));
        }
    }
}
