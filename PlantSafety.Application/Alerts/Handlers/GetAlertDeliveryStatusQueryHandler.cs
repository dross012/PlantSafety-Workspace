// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetAlertDeliveryStatusQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Alerts.Models;
using PlantSafety.Application.Alerts.Queries;
using PlantSafety.Application.Alerts.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Handlers;

/// <summary>Handles <see cref="GetAlertDeliveryStatusQuery"/>.</summary>
public sealed class GetAlertDeliveryStatusQueryHandler : IRequestHandler<GetAlertDeliveryStatusQuery, Result<IReadOnlyList<AlertDeliveryRecordDto>>>
{
    private readonly IAlertQueryRepository _repository;
    private readonly ILogger<GetAlertDeliveryStatusQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAlertDeliveryStatusQueryHandler(IAlertQueryRepository repository, ILogger<GetAlertDeliveryStatusQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<AlertDeliveryRecordDto>>> Handle(GetAlertDeliveryStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var records = await _repository.GetDeliveryRecordsAsync(request.AlertId, cancellationToken);
            return Result<IReadOnlyList<AlertDeliveryRecordDto>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve delivery status for Alert {AlertId}", request.AlertId);

            return Result<IReadOnlyList<AlertDeliveryRecordDto>>.Failure(new ApplicationError(
                "Alerts.Alert.DeliveryStatusRetrievalFailed", "Unable to retrieve delivery status at this time.", Guid.NewGuid()));
        }
    }
}
