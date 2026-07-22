// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetDashboardByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Dashboards.Dashboard.Models;
using PlantSafety.Application.Dashboards.Dashboard.Queries;
using PlantSafety.Application.Dashboards.Dashboard.Repositories;

namespace PlantSafety.Application.Dashboards.Dashboard.Handlers;

/// <summary>Handles <see cref="GetDashboardByIdQuery"/>.</summary>
public sealed class GetDashboardByIdQueryHandler : IRequestHandler<GetDashboardByIdQuery, Result<DashboardDto>>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<GetDashboardByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDashboardByIdQueryHandler(IDashboardRepository repository, ILogger<GetDashboardByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DashboardDto>> Handle(GetDashboardByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dashboard = await _repository.GetDtoByIdAsync(request.TenantId, request.DashboardId, cancellationToken);

            return dashboard is null
                ? Result<DashboardDto>.Failure(new ApplicationError(
                    "Dashboards.Dashboard.NotFound", $"Dashboard '{request.DashboardId}' was not found.", Guid.NewGuid(), ErrorSeverity.NotFound))
                : Result<DashboardDto>.Success(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Dashboard {DashboardId} for Tenant {TenantId}", request.DashboardId, request.TenantId);

            return Result<DashboardDto>.Failure(new ApplicationError(
                "Dashboards.Dashboard.RetrievalFailed", "Unable to retrieve the dashboard at this time.", Guid.NewGuid()));
        }
    }
}
