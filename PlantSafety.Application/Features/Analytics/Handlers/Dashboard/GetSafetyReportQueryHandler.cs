// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetSafetyReportQuery.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.Analytics.Queries.Reporting;

namespace PlantSafety.Application.Features.Analytics.Handlers.Dashboard;

/// <summary>Handles <see cref="GetSafetyReportQuery"/>. Returns null when no data exists for the period.</summary>
public sealed class GetSafetyReportQueryHandler : IQueryHandler<GetSafetyReportQuery, SafetyReportDto?>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<GetSafetyReportQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSafetyReportQueryHandler(IDashboardRepository repository, ILogger<GetSafetyReportQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SafetyReportDto?> Handle(GetSafetyReportQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetReportAsync(
            tenantId: request.TenantId,
            siteId: request.SiteId,
            fromUtc: request.FromUtc,
            toUtc: request.ToUtc,
            cancellationToken: cancellationToken);

        if (model is null)
        {
            _logger.LogDebug("Report data not found for Tenant {TenantId}", request.TenantId);
            return null;
        }

        return new SafetyReportDto
        {
            TenantId = model.TenantId,
            SiteId = model.SiteId,
            FromUtc = model.FromUtc,
            ToUtc = model.ToUtc,
            Trir = model.Trir,
            Dart = model.Dart,
            TrainingComplianceRate = model.TrainingComplianceRate,
            CultureScore = model.CultureScore,
        };
    }
}
