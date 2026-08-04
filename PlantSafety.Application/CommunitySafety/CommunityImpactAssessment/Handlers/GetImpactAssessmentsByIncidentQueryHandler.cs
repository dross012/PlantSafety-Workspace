// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetImpactAssessmentsByIncidentQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Queries;
using PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Repositories;

namespace PlantSafety.Application.CommunitySafety.CommunityImpactAssessment.Handlers;

/// <summary>Handles <see cref="GetImpactAssessmentsByIncidentQuery"/>.</summary>
public sealed class GetImpactAssessmentsByIncidentQueryHandler : IRequestHandler<GetImpactAssessmentsByIncidentQuery, Result<CommunityImpactAssessmentsQueryResult>>
{
    private readonly ICommunityImpactAssessmentRepository _repository;
    private readonly ILogger<GetImpactAssessmentsByIncidentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetImpactAssessmentsByIncidentQueryHandler(ICommunityImpactAssessmentRepository repository, ILogger<GetImpactAssessmentsByIncidentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityImpactAssessmentsQueryResult>> Handle(GetImpactAssessmentsByIncidentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetImpactAssessmentItemsByIncidentAsync(request.TenantId, request.IncidentId, cancellationToken);

            return Result<CommunityImpactAssessmentsQueryResult>.Success(new CommunityImpactAssessmentsQueryResult(
                request.TenantId, request.IncidentId, DateTime.UtcNow, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CommunityImpactAssessment records for Incident {IncidentId}", request.IncidentId);

            return Result<CommunityImpactAssessmentsQueryResult>.Failure(new ApplicationError(
                "CommunitySafety.CommunityImpactAssessment.RetrievalFailed",
                "Unable to retrieve CommunityImpactAssessment records at this time.",
                Guid.NewGuid()));
        }
    }
}
