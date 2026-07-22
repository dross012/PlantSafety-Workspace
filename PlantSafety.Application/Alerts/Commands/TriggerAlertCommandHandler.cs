// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for TriggerAlertCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Alerts.Alert.Enums;
using PlantSafety.Domain.Alerts.Alert.ValueObjects;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Handles <see cref="TriggerAlertCommand"/>.</summary>
public sealed class TriggerAlertCommandHandler : IRequestHandler<TriggerAlertCommand, Result<TriggerAlertResult>>
{
    private readonly IAlertCommandRepository _repository;
    private readonly ILogger<TriggerAlertCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public TriggerAlertCommandHandler(IAlertCommandRepository repository, ILogger<TriggerAlertCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TriggerAlertResult>> Handle(TriggerAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!AlertCategory.TryParse(request.CategoryCode, out var category) || category is null)
            {
                return Result<TriggerAlertResult>.Failure(new ApplicationError(
                    "Alerts.Alert.InvalidCategory",
                    $"Unknown alert category code '{request.CategoryCode}'.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            if (!Enum.TryParse<AlertSourceType>(request.SourceTypeCode, ignoreCase: true, out var sourceType))
            {
                return Result<TriggerAlertResult>.Failure(new ApplicationError(
                    "Alerts.Alert.InvalidSourceType",
                    $"Unknown alert source type '{request.SourceTypeCode}'.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            AlertSeverity? severity = null;
            if (!string.IsNullOrWhiteSpace(request.SeverityCode))
            {
                if (!AlertSeverity.TryParse(request.SeverityCode, out severity) || severity is null)
                {
                    return Result<TriggerAlertResult>.Failure(new ApplicationError(
                        "Alerts.Alert.InvalidSeverity",
                        $"Unknown alert severity code '{request.SeverityCode}'.",
                        request.CorrelationId,
                        ErrorSeverity.Validation));
                }
            }

            var location = string.IsNullOrWhiteSpace(request.LocationZoneName)
                ? AlertLocation.Unknown
                : AlertLocation.Create(request.LocationZoneName, request.Latitude, request.Longitude);

            var aggregate = PlantSafety.Domain.Alerts.Alert.Aggregates.Alert.Trigger(
                UtcTimestamp.From(request.RequestedAtUtc),
                new CorrelationId(request.CorrelationId),
                request.CausationId.HasValue ? new CausationId(request.CausationId.Value) : CausationId.New(),
                request.TriggeredByUserId.HasValue ? PerformedByActor.User(UserId.FromGuid(request.TriggeredByUserId.Value)) : PerformedByActor.System(),
                request.TenantId,
                request.SiteId,
                category,
                sourceType,
                request.Title,
                location,
                severity,
                request.TriggeredByUserId.HasValue ? UserId.FromGuid(request.TriggeredByUserId.Value) : null);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<TriggerAlertResult>.Success(new TriggerAlertResult(
                aggregate.AlertId.Value, aggregate.Status.ToString(), aggregate.Severity.Code));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger Alert for Tenant {TenantId}", request.TenantId);

            return Result<TriggerAlertResult>.Failure(new ApplicationError(
                "Alerts.Alert.TriggerFailed",
                "Unable to trigger the alert at this time.",
                request.CorrelationId));
        }
    }
}
