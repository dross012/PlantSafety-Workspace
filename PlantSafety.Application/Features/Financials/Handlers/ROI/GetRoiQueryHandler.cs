// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-15
// FILE DESCRIPTION: Handler for GetRoiQuery. Returns null when not found.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.Financials.Models.ROI;
using PlantSafety.Application.Features.Financials.Queries.ROI.GetROI;

namespace PlantSafety.Application.Features.Financials.Handlers.ROI;

/// <summary>Handles <see cref="GetRoiQuery"/>.</summary>
public sealed class GetRoiQueryHandler : IQueryHandler<GetRoiQuery, SafetyROIDto?>
{
    private readonly IRoiRepository _repository;
    private readonly ILogger<GetRoiQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRoiQueryHandler(IRoiRepository repository, ILogger<GetRoiQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SafetyROIDto?> Handle(GetRoiQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByFiscalYearAsync(request.TenantId, request.SiteId, request.FiscalYear, cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("ROI not found for Tenant {TenantId} FiscalYear {FiscalYear}", request.TenantId, request.FiscalYear);
            return null;
        }

        return new SafetyROIDto
        {
            RoiCalculationId = model.RoiCalculationId,
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            FiscalYear = model.FiscalYear,
            TotalInvestment = model.TotalInvestment,
            EstimatedAvoidedCosts = model.EstimatedAvoidedCosts,
            NetBenefit = model.NetBenefit,
            RoiPercentage = model.RoiPercentage,
            CurrencyCode = model.CurrencyCode,
            CalculatedAtUtc = model.CalculatedAtUtc,
            ETag = model.ETag,
        };
    }
}
