// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetSchedulingDashboardQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Repositories;

namespace PlantSafety.Application.Analytics.Dashboard.SchedulingDashboard.Handlers;

/// <summary>Handles <see cref="GetSchedulingDashboardQuery"/>.</summary>
public sealed class GetSchedulingDashboardQueryHandler : IRequestHandler<GetSchedulingDashboardQuery, Result<SchedulingDashboardQueryResult>>
{
    private readonly ISchedulingDashboardRepository _repository;
    private readonly ILogger<GetSchedulingDashboardQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSchedulingDashboardQueryHandler(ISchedulingDashboardRepository repository, ILogger<GetSchedulingDashboardQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SchedulingDashboardQueryResult>> Handle(GetSchedulingDashboardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetSchedulingDashboardAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<SchedulingDashboardQueryResult>.Success(new SchedulingDashboardQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve SchedulingDashboard for Tenant {TenantId}", request.TenantId);

            return Result<SchedulingDashboardQueryResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.SchedulingDashboard.RetrievalFailed",
                "Unable to retrieve Scheduling dashboard data at this time.",
                Guid.NewGuid()));
        }
    }
}
