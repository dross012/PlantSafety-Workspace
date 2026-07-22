// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetActiveAlertsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Alerts.Queries;
using PlantSafety.Application.Alerts.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Handlers;

/// <summary>Handles <see cref="GetActiveAlertsQuery"/>.</summary>
public sealed class GetActiveAlertsQueryHandler : IRequestHandler<GetActiveAlertsQuery, Result<GetActiveAlertsResult>>
{
    private readonly IAlertQueryRepository _repository;
    private readonly ILogger<GetActiveAlertsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetActiveAlertsQueryHandler(IAlertQueryRepository repository, ILogger<GetActiveAlertsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetActiveAlertsResult>> Handle(GetActiveAlertsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetActiveAlertsAsync(request.TenantId, request.SiteId, cancellationToken);

            return Result<GetActiveAlertsResult>.Success(new GetActiveAlertsResult(
                request.TenantId, request.SiteId, DateTime.UtcNow, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve active Alerts for Tenant {TenantId}", request.TenantId);

            return Result<GetActiveAlertsResult>.Failure(new ApplicationError(
                "Alerts.Alert.ActiveRetrievalFailed", "Unable to retrieve active alerts at this time.", Guid.NewGuid()));
        }
    }
}
