// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Read-only repository abstraction for DischargeMonitoringReport data. Implemented in the Infrastructure layer.
// SchemaVersion: 1

using PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Models;

namespace PlantSafety.Application.EnvironmentalHealth.WaterDischargeCompliance.DischargeMonitoringReport.Repositories;

/// <summary>Read-only repository for DischargeMonitoringReport data. Implemented in the Infrastructure layer.</summary>
public interface IDischargeMonitoringReportRepository
{
    /// <summary>Backs GetDischargeMonitoringReportsByPeriodQuery.</summary>
    Task<IReadOnlyList<DischargeMonitoringReportItemDto>> GetDischargeMonitoringReportsByPeriodAsync(Guid tenantId, Guid? siteId, DateTime periodStartUtc, DateTime periodEndUtc, CancellationToken cancellationToken);
}