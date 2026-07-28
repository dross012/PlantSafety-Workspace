// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for ListRoiHistoryQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Extensions;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Features.Financials.Models.ROI;
using PlantSafety.Application.Features.Financials.Queries.ROI.ListROIHistory;

namespace PlantSafety.Application.Features.Financials.Handlers.ROI;

/// <summary>Handles <see cref="ListRoiHistoryQuery"/>. Returns paginated ROI calculation history.</summary>
public sealed class ListRoiHistoryQueryHandler : IQueryHandler<ListRoiHistoryQuery, PaginatedResult<SafetyROIDto>>
{
    private readonly IRoiRepository _repository;
    private readonly ILogger<ListRoiHistoryQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListRoiHistoryQueryHandler(IRoiRepository repository, ILogger<ListRoiHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<PaginatedResult<SafetyROIDto>> Handle(ListRoiHistoryQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber.ClampPageNumber();
        var pageSize = request.PageSize.ClampPageSize();

        var (models, total) = await _repository.ListAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            orderBy: request.OrderBy.SafeOrderBy("FiscalYear"),
            sortDirection: request.SortDirection.SafeSortDirection(),
            pageNumber: pageNumber,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        var items = models.Select(m => new SafetyROIDto
        {
            RoiCalculationId = m.RoiCalculationId,
            TenantId = m.TenantId,
            SiteId = m.SiteId,
            FiscalYear = m.FiscalYear,
            TotalInvestment = m.TotalInvestment,
            EstimatedAvoidedCosts = m.EstimatedAvoidedCosts,
            NetBenefit = m.NetBenefit,
            RoiPercentage = m.RoiPercentage,
            CurrencyCode = m.CurrencyCode,
            CalculatedAtUtc = m.CalculatedAtUtc,
            ETag = m.ETag,
        }).ToList();

        return new PaginatedResult<SafetyROIDto>(items, total, pageNumber, pageSize);
    }
}
