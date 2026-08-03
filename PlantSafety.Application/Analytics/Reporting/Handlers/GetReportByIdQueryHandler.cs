// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetReportByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Analytics.Reporting.Models;
using PlantSafety.Application.Analytics.Reporting.Queries;
using PlantSafety.Application.Analytics.Reporting.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Reporting.Handlers;

/// <summary>Handles <see cref="GetReportByIdQuery"/>.</summary>
public sealed class GetReportByIdQueryHandler : IRequestHandler<GetReportByIdQuery, Result<ReportDetailDto>>
{
    private readonly IReportingRepository _repository;
    private readonly ILogger<GetReportByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetReportByIdQueryHandler(IReportingRepository repository, ILogger<GetReportByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ReportDetailDto>> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _repository.GetReportByIdAsync(request.TenantId, request.ReportId, cancellationToken);

            return report is null
                ? Result<ReportDetailDto>.Failure(new ApplicationError(
                    "Analytics.Reporting.NotFound", $"Report '{request.ReportId}' was not found.", Guid.NewGuid(), ErrorSeverity.NotFound))
                : Result<ReportDetailDto>.Success(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve report {ReportId} for Tenant {TenantId}", request.ReportId, request.TenantId);

            return Result<ReportDetailDto>.Failure(new ApplicationError(
                "Analytics.Reporting.DetailRetrievalFailed", "Unable to retrieve the report at this time.", Guid.NewGuid()));
        }
    }
}
