// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetJobSafetyAnalysisQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.JobSafetyAnalysis.Queries;
using PlantSafety.Application.Operational.JobSafetyAnalysis.Repositories;

namespace PlantSafety.Application.Operational.JobSafetyAnalysis.Handlers;

/// <summary>Handles <see cref="GetJobSafetyAnalysisQuery"/>.</summary>
public sealed class GetJobSafetyAnalysisQueryHandler : IRequestHandler<GetJobSafetyAnalysisQuery, Result<JobSafetyAnalysisQueryResult>>
{
    private readonly IJobSafetyAnalysisRepository _repository;
    private readonly ILogger<GetJobSafetyAnalysisQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetJobSafetyAnalysisQueryHandler(IJobSafetyAnalysisRepository repository, ILogger<GetJobSafetyAnalysisQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<JobSafetyAnalysisQueryResult>> Handle(GetJobSafetyAnalysisQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetJobSafetyAnalysisAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<JobSafetyAnalysisQueryResult>.Success(new JobSafetyAnalysisQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve JobSafetyAnalysis for Tenant {TenantId}", request.TenantId);

            return Result<JobSafetyAnalysisQueryResult>.Failure(new ApplicationError(
                "Operational.JobSafetyAnalysis.RetrievalFailed",
                "Unable to retrieve JobSafetyAnalysis data at this time.",
                Guid.NewGuid()));
        }
    }
}
