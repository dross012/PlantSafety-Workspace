// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the RoutineSafetyAuditsIncidentReporting read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Models;

namespace PlantSafety.Application.SafetyProcedures.RoutineSafetyAuditsIncidentReporting.Repositories;

/// <summary>Read-only repository for RoutineSafetyAuditsIncidentReporting data. Implemented in the Infrastructure layer.</summary>
public interface IRoutineSafetyAuditsIncidentReportingRepository
{
    /// <summary>Returns RoutineSafetyAuditsIncidentReporting items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<RoutineSafetyAuditsIncidentReportingItemDto>> GetRoutineSafetyAuditsIncidentReportingItemsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}