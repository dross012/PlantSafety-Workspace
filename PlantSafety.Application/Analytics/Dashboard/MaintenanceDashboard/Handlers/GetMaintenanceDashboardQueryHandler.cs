// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetMaintenanceDashboardQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Dashboard.MaintenanceDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.MaintenanceDashboard.Repositories;

namespace PlantSafety.Application.Analytics.Dashboard.MaintenanceDashboard.Handlers;

/// <summary>Handles <see cref="GetMaintenanceDashboardQuery"/>.</summary>
public sealed class GetMaintenanceDashboardQueryHandler : IRequestHandler<GetMaintenanceDashboardQuery, Result<MaintenanceDashboardQueryResult>>
{
    private readonly IMaintenanceDashboardRepository _repository;
    private readonly ILogger<GetMaintenanceDashboardQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetMaintenanceDashboardQueryHandler(IMaintenanceDashboardRepository repository, ILogger<GetMaintenanceDashboardQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MaintenanceDashboardQueryResult>> Handle(GetMaintenanceDashboardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetMaintenanceDashboardAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<MaintenanceDashboardQueryResult>.Success(new MaintenanceDashboardQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve MaintenanceDashboard for Tenant {TenantId}", request.TenantId);

            return Result<MaintenanceDashboardQueryResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.MaintenanceDashboard.RetrievalFailed",
                "Unable to retrieve Maintenance dashboard data at this time.",
                Guid.NewGuid()));
        }
    }
}
