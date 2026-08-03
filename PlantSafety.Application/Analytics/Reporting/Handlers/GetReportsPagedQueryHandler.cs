// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetReportsPagedQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Analytics.Reporting.Models;
using PlantSafety.Application.Analytics.Reporting.Queries;
using PlantSafety.Application.Analytics.Reporting.Repositories;
using PlantSafety.Application.Common.Constants;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Reporting.Handlers;

/// <summary>Handles <see cref="GetReportsPagedQuery"/>.</summary>
public sealed class GetReportsPagedQueryHandler : IRequestHandler<GetReportsPagedQuery, Result<PaginatedResult<ReportListItemDto>>>
{
    private readonly IReportingRepository _repository;
    private readonly ILogger<GetReportsPagedQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetReportsPagedQueryHandler(IReportingRepository repository, ILogger<GetReportsPagedQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<ReportListItemDto>>> Handle(GetReportsPagedQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber < 1 ? PaginationConstants.DefaultPageNumber : request.PageNumber;
            var pageSize = request.PageSize < PaginationConstants.MinPageSize
                ? PaginationConstants.DefaultPageSize
                : Math.Min(request.PageSize, PaginationConstants.MaxPageSize);

            var (items, totalCount) = await _repository.GetReportsPagedAsync(
                request.TenantId, request.SiteId, request.TypeFilter, pageNumber, pageSize, cancellationToken);

            return Result<PaginatedResult<ReportListItemDto>>.Success(new PaginatedResult<ReportListItemDto>(items, totalCount, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve paginated report catalog for Tenant {TenantId}", request.TenantId);

            return Result<PaginatedResult<ReportListItemDto>>.Failure(new ApplicationError(
                "Analytics.Reporting.ListRetrievalFailed", "Unable to retrieve the report catalog at this time.", Guid.NewGuid()));
        }
    }
}
