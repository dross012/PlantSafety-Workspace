// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve DischargeMonitoringReport read-model data.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Queries;

/// <summary>Retrieves DischargeMonitoringReport read-model data.</summary>
public sealed record GetDischargeMonitoringReportsByPeriodQuery(Guid TenantId, Guid? SiteId, DateTime PeriodStartUtc, DateTime PeriodEndUtc) : IRequest<Result<GetDischargeMonitoringReportsByPeriodQueryResult>>;

/// <summary>Result of <see cref="GetDischargeMonitoringReportsByPeriodQuery"/>.</summary>
public sealed record GetDischargeMonitoringReportsByPeriodQueryResult(IReadOnlyList<DischargeMonitoringReportItemDto> Items, DateTime GeneratedAtUtc);