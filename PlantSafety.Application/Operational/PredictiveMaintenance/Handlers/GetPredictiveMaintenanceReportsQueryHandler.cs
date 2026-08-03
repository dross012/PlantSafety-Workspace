// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.PredictiveMaintenance
//  Layer:          Application
//  File:           GetPredictiveMaintenanceReportsQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handles GetPredictiveMaintenanceReportsQuery by grouping real
//                  PredictiveMaintenanceAlert records into monthly rollups. No PDF/CSV export
//                  engine exists yet -- Format is informational only. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Constants;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PredictiveMaintenance.Models;
using PlantSafety.Application.Operational.PredictiveMaintenance.Queries;
using PlantSafety.Application.Operational.PredictiveMaintenance.Repositories;

namespace PlantSafety.Application.Operational.PredictiveMaintenance.Handlers;

/// <summary>Handles <see cref="GetPredictiveMaintenanceReportsQuery"/>.</summary>
public sealed class GetPredictiveMaintenanceReportsQueryHandler
    : IRequestHandler<GetPredictiveMaintenanceReportsQuery, Result<PaginatedResult<PredictiveMaintenanceReportDto>>>
{
    private readonly IPredictiveMaintenanceAlertRepository _repository;
    private readonly ILogger<GetPredictiveMaintenanceReportsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPredictiveMaintenanceReportsQueryHandler(
        IPredictiveMaintenanceAlertRepository repository,
        ILogger<GetPredictiveMaintenanceReportsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<PredictiveMaintenanceReportDto>>> Handle(GetPredictiveMaintenanceReportsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (items, _) = await _repository.GetPagedAsync(
                request.TenantId, equipmentId: null, activeOnly: false, pageNumber: 1, pageSize: PaginationConstants.MaxPageSize, cancellationToken);

            var now = DateTime.UtcNow;

            var reports = items
                .GroupBy(i => new DateOnly(i.PredictedAtUtc.Year, i.PredictedAtUtc.Month, 1))
                .OrderByDescending(g => g.Key)
                .Select(g =>
                {
                    var periodStart = g.Key;
                    var periodEnd = periodStart.AddMonths(1).AddDays(-1);
                    var seed = $"{request.TenantId}-{periodStart:yyyyMM}";

                    return new PredictiveMaintenanceReportDto
                    {
                        ReportId = DeterministicGuid(seed),
                        Title = $"Predictive Maintenance Report - {periodStart:MMMM yyyy}",
                        PeriodStart = periodStart,
                        PeriodEnd = periodEnd,
                        GeneratedAtUtc = now,
                        Format = "PDF",
                    };
                })
                .ToList();

            var pageNumber = Math.Max(1, request.PageNumber);
            var pageSize = Math.Max(1, request.PageSize);
            var page = reports.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Result<PaginatedResult<PredictiveMaintenanceReportDto>>.Success(
                new PaginatedResult<PredictiveMaintenanceReportDto>(page, reports.Count, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build PredictiveMaintenance reports for Tenant {TenantId}", request.TenantId);

            return Result<PaginatedResult<PredictiveMaintenanceReportDto>>.Failure(new ApplicationError(
                "Operational.PredictiveMaintenance.Reports.RetrievalFailed",
                "Unable to retrieve predictive maintenance reports at this time.",
                Guid.NewGuid()));
        }
    }

    /// <summary>Deterministic GUID derived from a stable seed string (MD5-based), so report identity is stable across calls.</summary>
    private static Guid DeterministicGuid(string seed)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(seed));
        return new Guid(bytes);
    }
}
