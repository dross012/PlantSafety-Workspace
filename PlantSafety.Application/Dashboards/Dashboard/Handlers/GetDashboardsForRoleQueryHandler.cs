// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetDashboardsForRoleQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Dashboard.Models;
using PlantSafety.Application.Dashboards.Dashboard.Queries;
using PlantSafety.Application.Dashboards.Dashboard.Repositories;

namespace PlantSafety.Application.Dashboards.Dashboard.Handlers;

/// <summary>Handles <see cref="GetDashboardsForRoleQuery"/>.</summary>
public sealed class GetDashboardsForRoleQueryHandler : IRequestHandler<GetDashboardsForRoleQuery, Result<IReadOnlyList<DashboardDto>>>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<GetDashboardsForRoleQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDashboardsForRoleQueryHandler(IDashboardRepository repository, ILogger<GetDashboardsForRoleQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<DashboardDto>>> Handle(GetDashboardsForRoleQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dashboards = await _repository.GetDashboardsForRoleAsync(request.TenantId, request.ViewerRole, cancellationToken);
            return Result<IReadOnlyList<DashboardDto>>.Success(dashboards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Dashboards for role {ViewerRole} and Tenant {TenantId}", request.ViewerRole, request.TenantId);

            return Result<IReadOnlyList<DashboardDto>>.Failure(new ApplicationError(
                "Dashboards.Dashboard.ByRoleRetrievalFailed", "Unable to retrieve dashboards for the given role at this time.", Guid.NewGuid()));
        }
    }
}
