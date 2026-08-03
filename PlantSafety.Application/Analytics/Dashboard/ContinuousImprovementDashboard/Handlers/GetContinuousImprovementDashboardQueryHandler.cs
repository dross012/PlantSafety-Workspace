// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Handler for GetContinuousImprovementDashboardQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Queries;
using PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Repositories;

namespace PlantSafety.Application.Analytics.Dashboard.ContinuousImprovementDashboard.Handlers;

/// <summary>Handles <see cref="GetContinuousImprovementDashboardQuery"/>.</summary>
public sealed class GetContinuousImprovementDashboardQueryHandler : IRequestHandler<GetContinuousImprovementDashboardQuery, Result<ContinuousImprovementDashboardQueryResult>>
{
    private readonly IContinuousImprovementDashboardRepository _repository;
    private readonly ILogger<GetContinuousImprovementDashboardQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetContinuousImprovementDashboardQueryHandler(IContinuousImprovementDashboardRepository repository, ILogger<GetContinuousImprovementDashboardQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ContinuousImprovementDashboardQueryResult>> Handle(GetContinuousImprovementDashboardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetContinuousImprovementDashboardAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ContinuousImprovementDashboardQueryResult>.Success(new ContinuousImprovementDashboardQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ContinuousImprovementDashboard for Tenant {TenantId}", request.TenantId);

            return Result<ContinuousImprovementDashboardQueryResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.ContinuousImprovementDashboard.RetrievalFailed",
                "Unable to retrieve ContinuousImprovement dashboard data at this time.",
                Guid.NewGuid()));
        }
    }
}
