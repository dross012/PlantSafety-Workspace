// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetContinuousMonitoringReviewQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.ContinuousMonitoringReview.Queries;
using PlantSafety.Application.RiskManagement.ContinuousMonitoringReview.Repositories;

namespace PlantSafety.Application.RiskManagement.ContinuousMonitoringReview.Handlers;

/// <summary>Handles <see cref="GetContinuousMonitoringReviewQuery"/>.</summary>
public sealed class GetContinuousMonitoringReviewQueryHandler : IRequestHandler<GetContinuousMonitoringReviewQuery, Result<ContinuousMonitoringReviewQueryResult>>
{
    private readonly IContinuousMonitoringReviewRepository _repository;
    private readonly ILogger<GetContinuousMonitoringReviewQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetContinuousMonitoringReviewQueryHandler(IContinuousMonitoringReviewRepository repository, ILogger<GetContinuousMonitoringReviewQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ContinuousMonitoringReviewQueryResult>> Handle(GetContinuousMonitoringReviewQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetContinuousMonitoringReviewAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ContinuousMonitoringReviewQueryResult>.Success(new ContinuousMonitoringReviewQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ContinuousMonitoringReview for Tenant {TenantId}", request.TenantId);

            return Result<ContinuousMonitoringReviewQueryResult>.Failure(new ApplicationError(
                "RiskManagement.ContinuousMonitoringReview.RetrievalFailed",
                "Unable to retrieve ContinuousMonitoringReview data at this time.",
                Guid.NewGuid()));
        }
    }
}