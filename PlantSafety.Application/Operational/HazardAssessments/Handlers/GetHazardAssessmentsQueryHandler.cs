// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetHazardAssessmentsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.HazardAssessments.Queries;
using PlantSafety.Application.Operational.HazardAssessments.Repositories;

namespace PlantSafety.Application.Operational.HazardAssessments.Handlers;

/// <summary>Handles <see cref="GetHazardAssessmentsQuery"/>.</summary>
public sealed class GetHazardAssessmentsQueryHandler : IRequestHandler<GetHazardAssessmentsQuery, Result<HazardAssessmentsQueryResult>>
{
    private readonly IHazardAssessmentsRepository _repository;
    private readonly ILogger<GetHazardAssessmentsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazardAssessmentsQueryHandler(IHazardAssessmentsRepository repository, ILogger<GetHazardAssessmentsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardAssessmentsQueryResult>> Handle(GetHazardAssessmentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHazardAssessmentsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HazardAssessmentsQueryResult>.Success(new HazardAssessmentsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazardAssessments for Tenant {TenantId}", request.TenantId);

            return Result<HazardAssessmentsQueryResult>.Failure(new ApplicationError(
                "Operational.HazardAssessments.RetrievalFailed",
                "Unable to retrieve HazardAssessments data at this time.",
                Guid.NewGuid()));
        }
    }
}
