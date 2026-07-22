// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Hazard Identification & Measurement read-model.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.HazardIdentificationMeasurement.Models;

namespace PlantSafety.Application.RiskManagement.HazardIdentificationMeasurement.Repositories;

/// <summary>Read-only repository for Hazard Identification & Measurement data. Implemented in the Infrastructure layer.</summary>
public interface IHazardIdentificationMeasurementRepository
{
    /// <summary>Returns Hazard Identification & Measurement items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<HazardIdentificationMeasurementItemDto>> GetHazardIdentificationMeasurementAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}