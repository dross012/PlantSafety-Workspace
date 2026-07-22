// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetNearMissReportingRateQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Repositories;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.NearMissReportingRate.Handlers;

/// <summary>Handles <see cref="GetNearMissReportingRateQuery"/>.</summary>
public sealed class GetNearMissReportingRateQueryHandler : IRequestHandler<GetNearMissReportingRateQuery, Result<NearMissReportingRateQueryResult>>
{
    private readonly INearMissReportingRateRepository _repository;
    private readonly ILogger<GetNearMissReportingRateQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetNearMissReportingRateQueryHandler(INearMissReportingRateRepository repository, ILogger<GetNearMissReportingRateQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<NearMissReportingRateQueryResult>> Handle(GetNearMissReportingRateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<NearMissReportingRateQueryResult>.Success(new NearMissReportingRateQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve NearMissReportingRate for Tenant {TenantId}", request.TenantId);

            return Result<NearMissReportingRateQueryResult>.Failure(new ApplicationError(
                "SafetyCulture.CultureAssessment.NearMissReportingRate.RetrievalFailed",
                "Unable to retrieve Near-Miss Reporting Rate data at this time.",
                Guid.NewGuid()));
        }
    }
}