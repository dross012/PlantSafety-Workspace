// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetAlertHistoryQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Alerts.Models;
using PlantSafety.Application.Alerts.Queries;
using PlantSafety.Application.Alerts.Repositories;
using PlantSafety.Application.Common.Constants;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Handlers;

/// <summary>Handles <see cref="GetAlertHistoryQuery"/>.</summary>
public sealed class GetAlertHistoryQueryHandler : IRequestHandler<GetAlertHistoryQuery, Result<PaginatedResult<AlertDto>>>
{
    private readonly IAlertQueryRepository _repository;
    private readonly ILogger<GetAlertHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAlertHistoryQueryHandler(IAlertQueryRepository repository, ILogger<GetAlertHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<AlertDto>>> Handle(GetAlertHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var pageNumber = request.PageNumber < 1 ? PaginationConstants.DefaultPageNumber : request.PageNumber;
            var pageSize = request.PageSize < PaginationConstants.MinPageSize
                ? PaginationConstants.DefaultPageSize
                : Math.Min(request.PageSize, PaginationConstants.MaxPageSize);

            var (items, totalCount) = await _repository.GetAlertHistoryAsync(
                request.TenantId, request.SiteId, request.From, request.To, pageNumber, pageSize, cancellationToken);

            return Result<PaginatedResult<AlertDto>>.Success(new PaginatedResult<AlertDto>(items, totalCount, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Alert history for Tenant {TenantId}", request.TenantId);

            return Result<PaginatedResult<AlertDto>>.Failure(new ApplicationError(
                "Alerts.Alert.HistoryRetrievalFailed", "Unable to retrieve alert history at this time.", Guid.NewGuid()));
        }
    }
}
