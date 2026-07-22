// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetHazardCommunicationAssessmentQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.HazardCommunicationAssessment.Queries;
using PlantSafety.Application.SafetyPolicies.HazardCommunicationAssessment.Repositories;

namespace PlantSafety.Application.SafetyPolicies.HazardCommunicationAssessment.Handlers;

/// <summary>Handles <see cref="GetHazardCommunicationAssessmentQuery"/>.</summary>
public sealed class GetHazardCommunicationAssessmentQueryHandler : IRequestHandler<GetHazardCommunicationAssessmentQuery, Result<HazardCommunicationAssessmentQueryResult>>
{
    private readonly IHazardCommunicationAssessmentRepository _repository;
    private readonly ILogger<GetHazardCommunicationAssessmentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazardCommunicationAssessmentQueryHandler(IHazardCommunicationAssessmentRepository repository, ILogger<GetHazardCommunicationAssessmentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardCommunicationAssessmentQueryResult>> Handle(GetHazardCommunicationAssessmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHazardCommunicationAssessmentAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HazardCommunicationAssessmentQueryResult>.Success(new HazardCommunicationAssessmentQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazardCommunicationAssessment for Tenant {TenantId}", request.TenantId);

            return Result<HazardCommunicationAssessmentQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.HazardCommunicationAssessment.RetrievalFailed",
                "Unable to retrieve HazardCommunicationAssessment data at this time.",
                Guid.NewGuid()));
        }
    }
}