// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetReportingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Reporting.Queries;
using PlantSafety.Application.Analytics.Reporting.Repositories;

namespace PlantSafety.Application.Analytics.Reporting.Handlers;

/// <summary>Handles <see cref="GetReportingQuery"/>.</summary>
public sealed class GetReportingQueryHandler : IRequestHandler<GetReportingQuery, Result<ReportingQueryResult>>
{
    private readonly IReportingRepository _repository;
    private readonly ILogger<GetReportingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetReportingQueryHandler(IReportingRepository repository, ILogger<GetReportingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ReportingQueryResult>> Handle(GetReportingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetReportItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ReportingQueryResult>.Success(new ReportingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Reporting for Tenant {TenantId}", request.TenantId);

            return Result<ReportingQueryResult>.Failure(new ApplicationError(
                "Analytics.Reporting.RetrievalFailed",
                "Unable to retrieve Reporting data at this time.",
                Guid.NewGuid()));
        }
    }
}
