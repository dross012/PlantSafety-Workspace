// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetRootCauseAnalysisQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Queries;
using PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Repositories;

namespace PlantSafety.Application.SafetyEvents.RootCauseAnalysis.Handlers;

/// <summary>Handles <see cref="GetRootCauseAnalysisQuery"/>.</summary>
public sealed class GetRootCauseAnalysisQueryHandler : IRequestHandler<GetRootCauseAnalysisQuery, Result<RootCauseAnalysisQueryResult>>
{
    private readonly IRootCauseAnalysisRepository _repository;
    private readonly ILogger<GetRootCauseAnalysisQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRootCauseAnalysisQueryHandler(IRootCauseAnalysisRepository repository, ILogger<GetRootCauseAnalysisQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RootCauseAnalysisQueryResult>> Handle(GetRootCauseAnalysisQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRootCauseAnalysesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RootCauseAnalysisQueryResult>.Success(new RootCauseAnalysisQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RootCauseAnalysis for Tenant {TenantId}", request.TenantId);

            return Result<RootCauseAnalysisQueryResult>.Failure(new ApplicationError(
                "SafetyEvents.RootCauseAnalysis.RetrievalFailed",
                "Unable to retrieve root cause analysis data at this time.",
                Guid.NewGuid()));
        }
    }
}
