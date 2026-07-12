// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetComplianceFailureQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Predictive.ComplianceFailure.Queries;
using PlantSafety.Application.Analytics.Predictive.ComplianceFailure.Repositories;

namespace PlantSafety.Application.Analytics.Predictive.ComplianceFailure.Handlers;

/// <summary>Handles <see cref="GetComplianceFailureQuery"/>.</summary>
public sealed class GetComplianceFailureQueryHandler : IRequestHandler<GetComplianceFailureQuery, Result<ComplianceFailureQueryResult>>
{
    private readonly IComplianceFailureRepository _repository;
    private readonly ILogger<GetComplianceFailureQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetComplianceFailureQueryHandler(IComplianceFailureRepository repository, ILogger<GetComplianceFailureQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ComplianceFailureQueryResult>> Handle(GetComplianceFailureQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetComplianceFailurePredictionsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ComplianceFailureQueryResult>.Success(new ComplianceFailureQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ComplianceFailure for Tenant {TenantId}", request.TenantId);

            return Result<ComplianceFailureQueryResult>.Failure(new ApplicationError(
                "Analytics.Predictive.ComplianceFailure.RetrievalFailed",
                "Unable to retrieve ComplianceFailure data at this time.",
                Guid.NewGuid()));
        }
    }
}
