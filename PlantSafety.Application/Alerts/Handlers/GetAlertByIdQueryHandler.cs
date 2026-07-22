// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetAlertByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Alerts.Models;
using PlantSafety.Application.Alerts.Queries;
using PlantSafety.Application.Alerts.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Handlers;

/// <summary>Handles <see cref="GetAlertByIdQuery"/>.</summary>
public sealed class GetAlertByIdQueryHandler : IRequestHandler<GetAlertByIdQuery, Result<AlertDto>>
{
    private readonly IAlertQueryRepository _repository;
    private readonly ILogger<GetAlertByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAlertByIdQueryHandler(IAlertQueryRepository repository, ILogger<GetAlertByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AlertDto>> Handle(GetAlertByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var alert = await _repository.GetByIdAsync(request.TenantId, request.AlertId, cancellationToken);

            return alert is null
                ? Result<AlertDto>.Failure(new ApplicationError(
                    "Alerts.Alert.NotFound", $"Alert '{request.AlertId}' was not found.", Guid.NewGuid(), ErrorSeverity.NotFound))
                : Result<AlertDto>.Success(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Alert {AlertId} for Tenant {TenantId}", request.AlertId, request.TenantId);

            return Result<AlertDto>.Failure(new ApplicationError(
                "Alerts.Alert.RetrievalFailed", "Unable to retrieve the alert at this time.", Guid.NewGuid()));
        }
    }
}
