// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetFacilityAssessmentQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditEngagement.FacilityAssessment.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.FacilityAssessment.Repositories;

namespace PlantSafety.Application.Auditing.AuditEngagement.FacilityAssessment.Handlers;

/// <summary>Handles <see cref="GetFacilityAssessmentQuery"/>.</summary>
public sealed class GetFacilityAssessmentQueryHandler : IRequestHandler<GetFacilityAssessmentQuery, Result<FacilityAssessmentQueryResult>>
{
    private readonly IFacilityAssessmentRepository _repository;
    private readonly ILogger<GetFacilityAssessmentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetFacilityAssessmentQueryHandler(IFacilityAssessmentRepository repository, ILogger<GetFacilityAssessmentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FacilityAssessmentQueryResult>> Handle(GetFacilityAssessmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<FacilityAssessmentQueryResult>.Success(new FacilityAssessmentQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve FacilityAssessment for Tenant {TenantId}", request.TenantId);

            return Result<FacilityAssessmentQueryResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.FacilityAssessment.RetrievalFailed",
                "Unable to retrieve Facility Assessment Auditing data at this time.",
                Guid.NewGuid()));
        }
    }
}