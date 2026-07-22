// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetIncidentTrackingPerformanceEvaluationQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Queries;
using PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Repositories;

namespace PlantSafety.Application.ContractorSafety.IncidentTrackingPerformanceEvaluation.Handlers;

/// <summary>Handles <see cref="GetIncidentTrackingPerformanceEvaluationQuery"/>.</summary>
public sealed class GetIncidentTrackingPerformanceEvaluationQueryHandler : IRequestHandler<GetIncidentTrackingPerformanceEvaluationQuery, Result<IncidentTrackingPerformanceEvaluationQueryResult>>
{
    private readonly IIncidentTrackingPerformanceEvaluationRepository _repository;
    private readonly ILogger<GetIncidentTrackingPerformanceEvaluationQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIncidentTrackingPerformanceEvaluationQueryHandler(IIncidentTrackingPerformanceEvaluationRepository repository, ILogger<GetIncidentTrackingPerformanceEvaluationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentTrackingPerformanceEvaluationQueryResult>> Handle(GetIncidentTrackingPerformanceEvaluationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetIncidentTrackingPerformanceEvaluationItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<IncidentTrackingPerformanceEvaluationQueryResult>.Success(new IncidentTrackingPerformanceEvaluationQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve IncidentTrackingPerformanceEvaluation records for Tenant {TenantId}", request.TenantId);

            return Result<IncidentTrackingPerformanceEvaluationQueryResult>.Failure(new ApplicationError(
                "ContractorSafety.IncidentTrackingPerformanceEvaluation.RetrievalFailed",
                "Unable to retrieve IncidentTrackingPerformanceEvaluation records at this time.",
                Guid.NewGuid()));
        }
    }
}