// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetReportingDistributionQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Scorecards.ReportingDistribution.Queries;
using PlantSafety.Application.Scorecards.ReportingDistribution.Repositories;

namespace PlantSafety.Application.Scorecards.ReportingDistribution.Handlers;

/// <summary>Handles <see cref="GetReportingDistributionQuery"/>.</summary>
public sealed class GetReportingDistributionQueryHandler : IRequestHandler<GetReportingDistributionQuery, Result<ReportingDistributionQueryResult>>
{
    private readonly IReportingDistributionRepository _repository;
    private readonly ILogger<GetReportingDistributionQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetReportingDistributionQueryHandler(IReportingDistributionRepository repository, ILogger<GetReportingDistributionQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ReportingDistributionQueryResult>> Handle(GetReportingDistributionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetReportingDistributionAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ReportingDistributionQueryResult>.Success(new ReportingDistributionQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ReportingDistribution for Tenant {TenantId}", request.TenantId);

            return Result<ReportingDistributionQueryResult>.Failure(new ApplicationError(
                "Scorecards.ReportingDistribution.RetrievalFailed",
                "Unable to retrieve ReportingDistribution data at this time.",
                Guid.NewGuid()));
        }
    }
}