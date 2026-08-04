// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           GetEsgDisclosureReportByIdQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handles GetEsgDisclosureReportByIdQuery. Returns a NotFound failure both when
//                  the row does not exist and when it belongs to a different tenant, so tenant
//                  membership is never revealed to an unauthorized caller. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Models;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Queries;
using PlantSafety.Application.Sustainability.AdvancedMetrics.Repositories;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Handlers;

/// <summary>Handles <see cref="GetEsgDisclosureReportByIdQuery"/>.</summary>
public sealed class GetEsgDisclosureReportByIdQueryHandler : IRequestHandler<GetEsgDisclosureReportByIdQuery, Result<EsgDisclosureReportDto>>
{
    private readonly IAdvancedMetricsReadRepository _repository;
    private readonly ILogger<GetEsgDisclosureReportByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEsgDisclosureReportByIdQueryHandler(IAdvancedMetricsReadRepository repository, ILogger<GetEsgDisclosureReportByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EsgDisclosureReportDto>> Handle(GetEsgDisclosureReportByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetEsgDisclosureReportByIdAsync(request.EsgDisclosureReportId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("EsgDisclosureReport {EsgDisclosureReportId} not found for Tenant {TenantId}", request.EsgDisclosureReportId, request.TenantId);

                return Result<EsgDisclosureReportDto>.Failure(new ApplicationError(
                    "Sustainability.AdvancedMetrics.EsgDisclosureReport.NotFound",
                    "The requested ESG disclosure report could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<EsgDisclosureReportDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ESG disclosure report {EsgDisclosureReportId}", request.EsgDisclosureReportId);

            return Result<EsgDisclosureReportDto>.Failure(new ApplicationError(
                "Sustainability.AdvancedMetrics.EsgDisclosureReport.RetrievalFailed",
                "Unable to retrieve ESG disclosure report data at this time.",
                Guid.NewGuid()));
        }
    }
}
