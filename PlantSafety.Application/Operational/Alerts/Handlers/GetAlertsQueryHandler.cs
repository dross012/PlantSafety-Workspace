// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetAlertsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.Alerts.Queries;
using PlantSafety.Application.Operational.Alerts.Repositories;

namespace PlantSafety.Application.Operational.Alerts.Handlers;

/// <summary>Handles <see cref="GetAlertsQuery"/>.</summary>
public sealed class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, Result<AlertsQueryResult>>
{
    private readonly IAlertsRepository _repository;
    private readonly ILogger<GetAlertsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAlertsQueryHandler(IAlertsRepository repository, ILogger<GetAlertsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AlertsQueryResult>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetAlertsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<AlertsQueryResult>.Success(new AlertsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Alerts for Tenant {TenantId}", request.TenantId);

            return Result<AlertsQueryResult>.Failure(new ApplicationError(
                "Operational.Alerts.RetrievalFailed",
                "Unable to retrieve Alerts data at this time.",
                Guid.NewGuid()));
        }
    }
}
