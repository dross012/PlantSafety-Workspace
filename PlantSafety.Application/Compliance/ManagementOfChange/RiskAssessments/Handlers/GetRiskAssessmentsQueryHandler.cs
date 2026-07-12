// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetRiskAssessmentsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.ManagementOfChange.RiskAssessments.Queries;
using PlantSafety.Application.Compliance.ManagementOfChange.RiskAssessments.Repositories;

namespace PlantSafety.Application.Compliance.ManagementOfChange.RiskAssessments.Handlers;

/// <summary>Handles <see cref="GetRiskAssessmentsQuery"/>.</summary>
public sealed class GetRiskAssessmentsQueryHandler : IRequestHandler<GetRiskAssessmentsQuery, Result<RiskAssessmentsQueryResult>>
{
    private readonly IRiskAssessmentsRepository _repository;
    private readonly ILogger<GetRiskAssessmentsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRiskAssessmentsQueryHandler(IRiskAssessmentsRepository repository, ILogger<GetRiskAssessmentsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RiskAssessmentsQueryResult>> Handle(GetRiskAssessmentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRiskAssessmentsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RiskAssessmentsQueryResult>.Success(new RiskAssessmentsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RiskAssessments for Tenant {TenantId}", request.TenantId);

            return Result<RiskAssessmentsQueryResult>.Failure(new ApplicationError(
                "Compliance.ManagementOfChange.RiskAssessments.RetrievalFailed",
                "Unable to retrieve Management of Change risk assessment data at this time.",
                Guid.NewGuid()));
        }
    }
}
