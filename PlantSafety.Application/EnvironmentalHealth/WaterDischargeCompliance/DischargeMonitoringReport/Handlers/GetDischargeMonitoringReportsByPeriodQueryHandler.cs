// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetDischargeMonitoringReportsByPeriodQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Queries;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Repositories;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Handlers;

/// <summary>Handles <see cref="GetDischargeMonitoringReportsByPeriodQuery"/>.</summary>
public sealed class GetDischargeMonitoringReportsByPeriodQueryHandler : IRequestHandler<GetDischargeMonitoringReportsByPeriodQuery, Result<GetDischargeMonitoringReportsByPeriodQueryResult>>
{
    private readonly IDischargeMonitoringReportRepository _repository;
    private readonly ILogger<GetDischargeMonitoringReportsByPeriodQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDischargeMonitoringReportsByPeriodQueryHandler(IDischargeMonitoringReportRepository repository, ILogger<GetDischargeMonitoringReportsByPeriodQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetDischargeMonitoringReportsByPeriodQueryResult>> Handle(GetDischargeMonitoringReportsByPeriodQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDischargeMonitoringReportsByPeriodAsync(request.TenantId, request.SiteId, request.PeriodStartUtc, request.PeriodEndUtc, cancellationToken);

            return Result<GetDischargeMonitoringReportsByPeriodQueryResult>.Success(new GetDischargeMonitoringReportsByPeriodQueryResult(items, DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute GetDischargeMonitoringReportsByPeriodQuery");

            return Result<GetDischargeMonitoringReportsByPeriodQueryResult>.Failure(new ApplicationError(
                "EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.GetDischargeMonitoringReportsByPeriodFailed",
                "Unable to retrieve DischargeMonitoringReport records at this time.",
                Guid.NewGuid()));
        }
    }
}